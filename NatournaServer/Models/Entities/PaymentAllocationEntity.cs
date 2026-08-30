using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatournaServer.Models.Entities
{
    /// <summary>
    /// Represents the allocation of a payment amount to a specific balance.
    /// A payment can be split across multiple balances (e.g., 20% to maintenance, 80% to utilities)
    /// </summary>
    public class PaymentAllocationEntity : BaseEntity, ITenantEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>Stamped automatically on insert; scoped by a global query filter.</summary>
        public int OrganizationId { get; set; }

        [Required]
        public int PaymentId { get; set; }

        [ForeignKey("PaymentId")]
        [JsonIgnore]
        public PaymentEntity? Payment { get; set; }

        [Required]
        public int BalanceId { get; set; }

        [ForeignKey("BalanceId")]
        [JsonIgnore]
        public BalanceEntity? Balance { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Percentage { get; set; }

        [Required]
        public decimal AllocatedAmount { get; set; }
    }
}
