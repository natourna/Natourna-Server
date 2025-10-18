using BuildingManagement.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BuildingManagement.Models.Entities
{
    public class PaymentEntity : BaseEntity
    {
        private int? _cycleId;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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

        public int? CycleId
        {
            get => _cycleId;
            set => _cycleId = value;
        }

        [ForeignKey("CycleId")]
        [JsonIgnore]
        public CycleEntity? Cycle { get; set; }

        [NotMapped]
        public bool Recurrent => CycleId.HasValue && CycleId.Value > 0;

        [JsonIgnore]
        public ICollection<PaymentAllocationEntity> PaymentAllocations { get; set; }

        public PaymentEntity(decimal amount, int apartmentId)
        {
            Amount = amount;
            ApartmentId = apartmentId;
            PaymentAllocations = [];
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
