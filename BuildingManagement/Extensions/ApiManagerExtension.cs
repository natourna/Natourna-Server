using BuildingManagement.Interfaces.Api;
using BuildingManagement.Services.Api;

namespace BuildingManagement.Extensions;

public static class ApiManagerExtension
{
    public static IServiceCollection AddApiManagers(this IServiceCollection services)
    {
        services.AddScoped<ICompoundApiManager, CompoundApiManager>();
        services.AddScoped<IBuildingApiManager, BuildingApiManager>();
        services.AddScoped<IApartmentApiManager, ApartmentApiManager>();
        services.AddScoped<IBillApiManager, BillApiManager>();
        services.AddScoped<IPaymentApiManager, PaymentApiManager>();
        services.AddScoped<IUserApiManager, UserApiManager>();

        return services;
    }
}