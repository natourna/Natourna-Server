using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Services;
using BuildingManagement.Services.Api;
using BuildingManagement.Services.Audit;

namespace BuildingManagement.Extensions;

public static class ApiManagerExtension
{
    public static IServiceCollection AddApiManagers(this IServiceCollection services)
    {
        services.AddScoped<IAuthApiManager, AuthApiManager>();
        services.AddScoped<ICompoundApiManager, CompoundApiManager>();
        services.AddScoped<IBuildingApiManager, BuildingApiManager>();
        services.AddScoped<IApartmentApiManager, ApartmentApiManager>();
        services.AddScoped<IBillApiManager, BillApiManager>();
        services.AddScoped<IPaymentApiManager, PaymentApiManager>();
        services.AddScoped<IUserApiManager, UserApiManager>();
        services.AddScoped<ICycleApiManager, CycleApiManager>();
        services.AddScoped<IBalanceApiManager, BalanceApiManager>();
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}