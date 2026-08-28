using NatrounaServer.Exceptions;
using NatrounaServer.Models.Configurations;

namespace NatrounaServer.Extensions
{
    public static class KestrelExtension
    {
        public static void AddListenPort(this ConfigureWebHostBuilder webHost, IConfiguration configuration)
        {
            int? port = configuration.Get<AppConfiguration>()?.NatrounaServer.Port;

            if (port == 0 || port == null)
            {
                throw new CustomException("APP-02", "NatrounaServer.Port argument is required.");
            }

            webHost.UseKestrel(opt =>
            {
                opt.AddServerHeader = false;
                opt.ListenAnyIP((int)port);
            });
        }
    }
}
