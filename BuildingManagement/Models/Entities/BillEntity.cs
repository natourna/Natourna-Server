using BuildingManagement.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BuildingManagement.Models.Entities
{
    public class BillEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Label { get; set; }

        [RequiredDecimal]
        public decimal Amount { get; set; }

        public decimal? AmmountPaid { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public bool? IsPaid { get; set; }

        [RequiredInt]
        public int CompoundId { get; set; }

        [ForeignKey("CompoundId")]
        [JsonIgnore]
        public CompoundEntity? Compound { get; set; }

        [JsonIgnore]
        public ICollection<PaymentEntity> Payments { get; set; }

        public BillEntity(int id, string label, decimal amount, bool? isPaid, int compoundId)
        {
            Id = id;
            Label = label;
            Amount = amount;
            IsPaid = isPaid ?? false;
            AmmountPaid = 0;
            CompoundId = compoundId;
            Payments = [];
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
