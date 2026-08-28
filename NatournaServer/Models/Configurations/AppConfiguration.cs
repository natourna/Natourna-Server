using NatournaServer.Models.Configurations.NatournaServer;

namespace NatournaServer.Models.Configurations
{
    public class AppConfiguration
    {
        public NatournaServerConfiguration NatournaServer { get; set; } = new();

        public JwtConfiguration Jwt { get; set; } = new();
    }
}
