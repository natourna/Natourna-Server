using BuildingManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BuildingManagement.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BuildingManagementContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
            
        return services;
    }
}