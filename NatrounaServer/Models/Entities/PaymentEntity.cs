using NatrounaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatrounaServer.Models.Entities
{
    public class PaymentEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Label { get; set; }

        [RequiredDecimal]
        public decimal Amount { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsPaid { get; set; } = false;

        [RequiredInt]
        public int ApartmentId { get; set; }

        [ForeignKey("ApartmentId")]
        [JsonIgnore]
        public ApartmentEntity? Apartment { get; set; }

        public int? CycleId { get; set; }

        [ForeignKey("CycleId")]
        [JsonIgnore]
        public CycleEntity? Cycle { get; set; }

        [NotMapped]
        public bool Recurrent => CycleId.HasValue && CycleId.Value > 0;

        [JsonIgnore]
        public ICollection<PaymentAllocationEntity> PaymentAllocations { get; set; }

        public PaymentEntity(string label, decimal amount, int apartmentId)
        {
            Label = label;
            Amount = amount;
            ApartmentId = apartmentId;
            PaymentAllocations = [];
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
