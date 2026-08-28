using NatournaServer.Models.Configurations.NatournaServer;

namespace NatournaServer.Models.Configurations
{
    public class AppConfiguration
    {
        public NatournaServerConfiguration NatournaServer { get; set; } = new();
    }
}
