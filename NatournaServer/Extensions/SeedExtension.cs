using NatournaServer.Constants.Subscription;
using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Authentication;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Configurations;
using NatournaServer.Models.Entities;
using Microsoft.Extensions.Options;

namespace NatournaServer.Extensions;

public static class SeedExtension
{
    public static IServiceCollection AddBootstrapConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BootstrapConfiguration>(configuration.GetSection("Bootstrap"));

        return services;
    }

    /// <summary>
    /// Ensure the reference roles exist before the application starts serving requests
    /// </summary>
    public static async Task SeedRolesAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleContextManager = scope.ServiceProvider.GetRequiredService<IRoleContextManager>();

        foreach (var name in new[] { RoleNames.User, RoleNames.Admin })
        {
            var existing = await roleContextManager.GetByNameAsync(name);
            if (existing == null)
            {
                await roleContextManager.CreateAsync(new RoleEntity(0, name));
            }
        }
    }

    /// <summary>
    /// Create the configured first admin when the users table is empty
    /// </summary>
    public static async Task SeedBootstrapAdminAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedExtension");
        var userContextManager = provider.GetRequiredService<IUserContextManager>();

        if (await userContextManager.AnyAsync())
        {
            return;
        }

        var bootstrap = provider.GetRequiredService<IOptions<BootstrapConfiguration>>().Value;

        if (string.IsNullOrEmpty(bootstrap.AdminEmail) || string.IsNullOrEmpty(bootstrap.AdminPassword))
        {
            logger.LogWarning("Users table is empty and no bootstrap admin is configured");
            return;
        }

        var roleContextManager = provider.GetRequiredService<IRoleContextManager>();
        var adminRole = await roleContextManager.GetByNameAsync(RoleNames.Admin);
        if (adminRole == null)
        {
            logger.LogWarning("Bootstrap admin skipped because the admin role is missing");
            return;
        }

        // The admin must belong to an organization: reuse the first one when the
        // database was seeded externally, otherwise create it (plus a trial subscription).
        var organizationContextManager = provider.GetRequiredService<IOrganizationContextManager>();
        var organization = await organizationContextManager.GetFirstAsync();

        if (organization == null)
        {
            organization = await organizationContextManager.CreateAsync(new OrganizationEntity(bootstrap.OrganizationName));

            var subscriptionContextManager = provider.GetRequiredService<ISubscriptionContextManager>();
            await subscriptionContextManager.CreateAsync(new SubscriptionEntity(organization.Id, SubscriptionStatus.Trial, 7m));

            logger.LogInformation("Seeded bootstrap organization {Name}", organization.Name);
        }

        var passwordHashingService = provider.GetRequiredService<IPasswordHashingService>();
        var passwordHash = passwordHashingService.HashPassword(bootstrap.AdminPassword);

        // Seeding runs outside a request (no tenant in scope), so the org is set explicitly
        await userContextManager.CreateAsync(new UserEntity(0, bootstrap.AdminEmail, passwordHash, string.Empty, adminRole.Id)
        {
            OrganizationId = organization.Id
        });

        logger.LogInformation("Seeded bootstrap admin {Email}", bootstrap.AdminEmail);
    }
}
