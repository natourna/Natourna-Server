using NatournaServer.Data;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Context;
using NatournaServer.Services.Context;
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
        services.AddScoped<IBalanceContextManager, BalanceContextManager>();
        services.AddScoped<ICycleContextManager, CycleContextManager>();
        services.AddScoped<ILogContextManager, AuditContextManager>();

        return services;
    }

    /// <summary>
    /// Ensure database is created and migrations are applied
    /// </summary>
    public static async Task AddContextService(this IServiceProvider services, bool isDev)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider;
        var context = service.GetRequiredService<NatournaServerContext>();

        if (context.Database.IsNpgsql())
        {
            if (isDev)
            {
                await context.Database.EnsureCreatedAsync();
            }
            else
            {
                await context.Database.MigrateAsync();
            }
        }
    }

    /// <summary>
    /// Add PostgreSQL database context to the service collection
    /// </summary>
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