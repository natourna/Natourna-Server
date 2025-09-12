using BuildingManagement.Configurations.BuildingManagement;

namespace BuildingManagement.Configurations
{
    public class AppConfiguration
    {
        public BuildingManagementConfiguration BuildingManagement { get; set; }

        public AppConfiguration(BuildingManagementConfiguration buildingManagement)
        {
            BuildingManagement = buildingManagement;
        }
    }
}
