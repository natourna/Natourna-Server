using NatrounaServer.Models.Configurations.NatrounaServer;

namespace NatrounaServer.Models.Configurations
{
    public class AppConfiguration
    {
        public NatrounaServerConfiguration NatrounaServer { get; set; } = new();

        public JwtConfiguration Jwt { get; set; } = new();
    }
}
