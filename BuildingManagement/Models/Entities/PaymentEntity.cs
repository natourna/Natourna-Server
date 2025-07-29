using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingManagement.Models.Entities
{
    public class PaymentEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool Recurrent { get; set; }

        public DateTime PaymentDate { get; set; }

        public int BillId { get; set; }

        public decimal Amount { get; set; }

        [ForeignKey("BillId")]
        public BillEntity? Bill { get; set; }

        public int ApartmentId { get; set; }

        [ForeignKey("ApartmentId")]
        public ApartmentEntity? Apartment { get; set; }

        public PaymentEntity(int id, bool recurrent, DateTime paymentDate, decimal amount, int billId, int apartmentId)
        {
            Id = id;
            Recurrent = recurrent;
            BillId = billId;
            PaymentDate = paymentDate;
            Amount = amount;
            ApartmentId = apartmentId;
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
