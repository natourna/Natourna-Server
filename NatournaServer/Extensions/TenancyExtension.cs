using NatournaServer.Interfaces.Tenancy;
using NatournaServer.Services.Tenancy;

namespace NatournaServer.Extensions;

public static class TenancyExtension
{
    public static IServiceCollection AddTenancy(this IServiceCollection services)
    {
        services.AddScoped<ITenantContext, HttpTenantContext>();

        return services;
    }
}
