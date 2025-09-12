using BuildingManagement.Data;
using BuildingManagement.Exceptions;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Services.Context;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Extensions;

public static class ContextManagerExtension
{
    public static IServiceCollection AddContextManagers(this IServiceCollection services)
    {
        services.AddScoped<ICompoundContextManager, CompoundContextManager>();
        services.AddScoped<IBuildingContextManager, BuildingContextManager>();
        services.AddScoped<IApartmentContextManager, ApartmentContextManager>();
        services.AddScoped<IBillContextManager, BillContextManager>();
        services.AddScoped<IPaymentContextManager, PaymentContextManager>();
        services.AddScoped<IUserContextManager, UserContextManager>();

        return services;
    }

    /// <summary>
    /// Ensure database is created and migrations are applied
    /// </summary>
    public static async Task AddContextService(this IServiceProvider services, bool isDev)
    {
        using var scope = services.CreateScope();

        var service = scope.ServiceProvider;
        var context = service.GetRequiredService<BuildingManagementContext>();

        if (context.Database.IsMySql())
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
    /// Add MySQL database context to the service collection
    /// </summary>
    public static void AddMySqlService(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new CustomException("MYSQL-CONTEXT-01", "Connection string 'DefaultConnection' not found.");
        }

        services.AddDbContext<BuildingManagementContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });
    }
}