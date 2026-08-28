using NatournaServer.Exceptions;
using NatournaServer.Models.Configurations;

namespace NatournaServer.Extensions
{
    public static class KestrelExtension
    {
        public static void AddListenPort(this ConfigureWebHostBuilder webHost, IConfiguration configuration)
        {
            int? port = configuration.Get<AppConfiguration>()?.NatournaServer.Port;

            if (port == 0 || port == null)
            {
                throw new CustomException("APP-02", "NatournaServer.Port argument is required.");
            }

            webHost.UseKestrel(opt =>
            {
                opt.AddServerHeader = false;
                opt.ListenAnyIP((int)port);
            });
        }
    }
}
