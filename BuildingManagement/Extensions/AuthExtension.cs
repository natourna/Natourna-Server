using Microsoft.AspNetCore.Authentication;

namespace BuildingManagement.Extensions
{
    public static class AuthExtension
    {
        public static void AddAuthenticationService(this IServiceCollection services)
        {
            services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
        }
    }
}
