using BuildingManagement.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BuildingManagement.Models.Entities
{
    public class PaymentEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public bool? Recurrent { get; set; }

        public DateTime? PaymentDate { get; set; }

        [RequiredInt]
        public int BillId { get; set; }

        [RequiredDecimal]
        public decimal Amount { get; set; }

        [ForeignKey("BillId")]
        [JsonIgnore]
        public BillEntity? Bill { get; set; }

        [RequiredInt]
        public int ApartmentId { get; set; }

        [ForeignKey("ApartmentId")]
        [JsonIgnore]
        public ApartmentEntity? Apartment { get; set; }

        public PaymentEntity(int id, bool? recurrent, DateTime? paymentDate, decimal amount, int billId, int apartmentId)
        {
            Id = id;
            Recurrent = recurrent ?? false;
            BillId = billId;
            PaymentDate = paymentDate ?? DateTime.UtcNow;
            Amount = amount;
            ApartmentId = apartmentId;
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
