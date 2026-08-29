using NatournaServer.Interfaces.Tenancy;
using NatournaServer.Models.Configurations;
using NatournaServer.Services.Tenancy;

namespace NatournaServer.Extensions;

public static class TenancyExtension
{
    public static IServiceCollection AddTenancy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITenantContext, HttpTenantContext>();

        // Self-service signup gate (off by default)
        services.Configure<RegistrationConfiguration>(configuration.GetSection("Registration"));

        return services;
    }
}
