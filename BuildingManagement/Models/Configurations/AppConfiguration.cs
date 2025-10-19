using BuildingManagement.Models.Configurations.BuildingManagement;

namespace BuildingManagement.Models.Configurations
{
    public class AppConfiguration
    {
        public BuildingManagementConfiguration BuildingManagement { get; set; } = new();

        public JwtConfiguration Jwt { get; set; } = new();
    }
}
