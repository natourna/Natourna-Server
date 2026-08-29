using System.ComponentModel.DataAnnotations;

namespace NatournaServer.Models.Api.Requests.Organization
{
    public class UpdateOrganizationSettingsRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// LBP per 1 USD, used by the client for dual currency display. Null hides LBP.
        /// </summary>
        [Range(0, 100000000, ErrorMessage = "Exchange rate must be a positive number")]
        public decimal? LbpExchangeRate { get; set; }
    }
}
