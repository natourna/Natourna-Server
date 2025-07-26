using BuildingManagement.Interfaces.Context;
using BuildingManagement.Services.Context;

namespace BuildingManagement.Extensions;

public static class ContextManagerExtensions
{
    public static IServiceCollection AddContextManagers(this IServiceCollection services)
    {
        services.AddScoped<ICompoundContextManager, CompoundContextManager>();
        services.AddScoped<IBuildingContextManager, BuildingContextManager>();
        services.AddScoped<IApartmentContextManager, ApartmentContextManager>();
        services.AddScoped<IBillContextManager, BillContextManager>();
        services.AddScoped<IPaymentContextManager, PaymentContextManager>();

        return services;
    }
}