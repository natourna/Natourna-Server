using NatournaServer.Constants.Subscription;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatournaServer.Models.Entities
{
    /// <summary>One per organization; monthly cost = PricePerBuilding x building count, computed and never stored.</summary>
    public class SubscriptionEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        [JsonIgnore]
        public OrganizationEntity? Organization { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Trial;

        /// <summary>USD per building per month.</summary>
        public decimal PricePerBuilding { get; set; } = 7m;

        public DateTime StartDate { get; set; }

        public SubscriptionEntity(int organizationId, SubscriptionStatus status, decimal pricePerBuilding)
        {
            OrganizationId = organizationId;
            Status = status;
            PricePerBuilding = pricePerBuilding;
            StartDate = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
