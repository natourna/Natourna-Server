using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Services;
using NatournaServer.Services.Api;
using NatournaServer.Services.Audit;

namespace NatournaServer.Extensions;

public static class ApiManagerExtension
{
    public static IServiceCollection AddApiManagers(this IServiceCollection services)
    {
        services.AddScoped<IAuthApiManager, AuthApiManager>();
        services.AddScoped<IOrganizationApiManager, OrganizationApiManager>();
        services.AddScoped<ICompoundApiManager, CompoundApiManager>();
        services.AddScoped<IBuildingApiManager, BuildingApiManager>();
        services.AddScoped<IApartmentApiManager, ApartmentApiManager>();
        services.AddScoped<IBillApiManager, BillApiManager>();
        services.AddScoped<IPaymentApiManager, PaymentApiManager>();
        services.AddScoped<IUserApiManager, UserApiManager>();
        services.AddScoped<IRoleApiManager, RoleApiManager>();
        services.AddScoped<ICycleApiManager, CycleApiManager>();
        services.AddScoped<IBalanceApiManager, BalanceApiManager>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}