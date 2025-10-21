using BuildingManagement.Models.Validation;
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

        public DateTime? DueDate { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime? PaymentDate { get; set; }

        [RequiredInt]
        public int BalanceId { get; set; }

        [ForeignKey("BalanceId")]
        [JsonIgnore]
        public BalanceEntity? Balance { get; set; }

        public BillEntity(string label, decimal amount, int balanceId)
        {
            Label = label;
            Amount = amount;
            BalanceId = balanceId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
