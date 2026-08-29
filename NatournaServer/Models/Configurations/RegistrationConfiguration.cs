namespace NatournaServer.Models.Configurations
{
    public class RegistrationConfiguration
    {
        /// <summary>
        /// Gates POST /api/Organization/register. Off by default - self-service
        /// signup opens only when the SaaS launch is ready.
        /// </summary>
        public bool Enabled { get; set; } = false;
    }
}
