using BuildingManagement.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace BuildingManagement.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Try to get Cloud SQL Unix socket env vars
        var unixSocket = Environment.GetEnvironmentVariable("INSTANCE_UNIX_SOCKET");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPass = Environment.GetEnvironmentVariable("DB_PASS");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");

        string? connectionString;
        if (!string.IsNullOrEmpty(unixSocket) && !string.IsNullOrEmpty(dbUser) && !string.IsNullOrEmpty(dbPass) && !string.IsNullOrEmpty(dbName))
        {
            // Build connection string for Cloud SQL Unix socket
            var builder = new MySqlConnectionStringBuilder
            {
                Server = unixSocket, // e.g. '/cloudsql/project:region:instance'
                UserID = dbUser,
                Password = dbPass,
                Database = dbName,
                ConnectionProtocol = MySqlConnectionProtocol.UnixSocket,
                SslMode = MySqlSslMode.Disabled,
                Pooling = true
            };
            connectionString = builder.ConnectionString;
        }
        else
        {
            // Fallback to config connection string
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        services.AddDbContext<BuildingManagementContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            ));
        return services;
    }
}