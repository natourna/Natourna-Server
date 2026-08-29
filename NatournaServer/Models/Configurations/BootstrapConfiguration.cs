namespace NatournaServer.Models.Configurations
{
    public class BootstrapConfiguration
    {
        public string AdminEmail { get; set; } = string.Empty;

        public string AdminPassword { get; set; } = string.Empty;

        /// <summary>
        /// Name of the organization created for the bootstrap admin when the
        /// database has none yet (an existing organization is reused instead).
        /// </summary>
        public string OrganizationName { get; set; } = "Default Organization";
    }
}
