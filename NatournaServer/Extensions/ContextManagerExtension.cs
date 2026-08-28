using NatournaServer.Constants.User;
using NatournaServer.Data;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;
using NatournaServer.Services.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Extensions;

public static class ContextManagerExtension
{
    public static IServiceCollection AddContextManagers(this IServiceCollection services)
    {
        services.AddScoped<ICompoundContextManager, CompoundContextManager>();
        services.AddScoped<IBuildingContextManager, BuildingContextManager>();
        services.AddScoped<IApartmentContextManager, ApartmentContextManager>();
        services.AddScoped<IBillContextManager, BillContextManager>();
        services.AddScoped<IPaymentContextManager, PaymentContextManager>();
        services.AddScoped<IPaymentAllocationContextManager, PaymentAllocationContextManager>();
        services.AddScoped<IUserContextManager, UserContextManager>();
        services.AddScoped<IRoleContextManager, RoleContextManager>();
        services.AddScoped<IBalanceContextManager, BalanceContextManager>();
        services.AddScoped<ICycleContextManager, CycleContextManager>();
        services.AddScoped<ILogContextManager, AuditContextManager>();

        return services;
    }

    public static async Task AddContextService(this IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<NatournaServerContext>();

        if (context.Database.IsNpgsql())
        {
            await context.Database.MigrateAsync();
        }

        await SeedAdminAsync(provider, configuration);
    }

    private static async Task SeedAdminAsync(IServiceProvider provider, IConfiguration configuration)
    {
        var context = provider.GetRequiredService<NatournaServerContext>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminBootstrap");

        if (await context.Users.AnyAsync())
        {
            return;
        }

        string? email = configuration["Bootstrap:AdminEmail"];
        string? password = configuration["Bootstrap:AdminPassword"];

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            logger.LogWarning("No users exist and Bootstrap:AdminEmail / Bootstrap:AdminPassword are not configured; nobody can log in");
            return;
        }

        var adminRole = await context.Roles.FirstAsync(r => r.Name == RoleNames.Admin);
        var hasher = provider.GetRequiredService<IPasswordHasher<UserEntity>>();

        var admin = new UserEntity(email, string.Empty, string.Empty, adminRole.Id);
        admin.Password = hasher.HashPassword(admin, password);

        context.Users.Add(admin);
        await context.SaveChangesAsync();

        logger.LogInformation("Bootstrapped initial admin user {Email}", email);
    }

    public static void AddPostgreSqlService(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new CustomException("POSTGRESQL-CONTEXT-01", "Connection string 'DefaultConnection' not found.");
        }

        services.AddDbContext<NatournaServerContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
    }
}
