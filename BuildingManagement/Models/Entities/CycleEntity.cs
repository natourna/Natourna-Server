using BuildingManagement.Constants.Cycle;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BuildingManagement.Models.Entities
{
    public class CycleEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Label { get; set; }

        public string? Description { get; set; }

        public PaymentCycle Cycle { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? ApartmentIdsCsv { get; set; }

        public decimal Amount { get; set; }

        public bool IsActive { get; set; } = true;

        public string? BalanceAllocationsJson { get; set; }

        [JsonIgnore]
        public ICollection<PaymentEntity> Payments { get; set; }

        public CycleEntity(string label, PaymentCycle cycle, DateTime startDate, DateTime endDate, decimal amount)
        {
            Label = label;
            Cycle = cycle;
            StartDate = startDate;
            EndDate = endDate;
            Amount = amount;
            IsActive = true;
            Payments = [];
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}