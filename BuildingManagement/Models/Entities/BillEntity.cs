using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingManagement.Models.Entities
{
    public class BillEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Label { get; set; }

        public decimal Amount { get; set; }

        public decimal AmmountPaid { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; }

        public int CompoundId { get; set; }

        [ForeignKey("CompoundId")]
        public CompoundEntity? Compound { get; set; }

        public ICollection<PaymentEntity> Payments { get; set; }

        public BillEntity(int id, string label, decimal amount, decimal ammountPaid, DateTime dueDate, bool isPaid, int compoundId)
        {
            Id = id;
            Label = label;
            Amount = amount;
            AmmountPaid = ammountPaid;
            DueDate = dueDate;
            IsPaid = isPaid;
            CompoundId = compoundId;
            Payments = [];
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
