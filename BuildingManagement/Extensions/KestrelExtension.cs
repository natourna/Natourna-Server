using BuildingManagement.Exceptions;
using BuildingManagement.Models.Configurations;

namespace BuildingManagement.Extensions
{
    public static class KestrelExtension
    {
        public static void AddListenPort(this ConfigureWebHostBuilder webHost, IConfiguration configuration)
        {
            int? port = configuration.Get<AppConfiguration>()?.BuildingManagement.Port;

            if (port == 0 || port == null)
            {
                throw new CustomException("APP-02", "BuildingManagement.Port argument is required.");
            }

            webHost.UseKestrel(opt =>
            {
                opt.AddServerHeader = false;
                opt.ListenAnyIP((int)port);
            });
        }
    }
}
