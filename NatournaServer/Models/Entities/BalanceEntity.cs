using NatournaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatournaServer.Models.Entities
{
    public class BalanceEntity : BaseEntity, ITenantEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>Stamped automatically on insert; scoped by a global query filter.</summary>
        public int OrganizationId { get; set; }

        [Required]
        public string Label { get; set; }

        public decimal CurrentAmount { get; set; } = 0;

        [RequiredInt]
        public int CompoundId { get; set; }

        [ForeignKey("CompoundId")]
        [JsonIgnore]
        public CompoundEntity? Compound { get; set; }

        [JsonIgnore]
        public ICollection<PaymentAllocationEntity> PaymentAllocations { get; set; }

        [JsonIgnore]
        public ICollection<BillEntity> Bills { get; set; }

        public BalanceEntity(string label, int compoundId)
        {
            CompoundId = compoundId;
            Label = label;
            PaymentAllocations = [];
            Bills = [];
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}