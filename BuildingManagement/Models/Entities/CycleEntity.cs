using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BuildingManagement.Constants.Cycle;

namespace BuildingManagement.Models.Entities
{
    /// <summary>
    /// Represents a recurring payment cycle for collective building expenses.
    /// E.g., Monthly payments for trash collection, hot water, maintenance, etc.
    /// Generates expected payments for apartments based on the cycle schedule.
    /// </summary>
    public class CycleEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Label/name for this cycle (e.g., "Monthly Building Fees", "Trash Collection")
        /// </summary>
        [Required]
        public string Label { get; set; }

        /// <summary>
        /// Description of what this cycle covers (e.g., "Includes trash, hot water, and common area maintenance")
        /// </summary>
        public string? Description { get; set; }

        public PaymentCycle Cycle { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        // null or empty means all apartments
        public string? ApartmentIdsCsv { get; set; }

        public decimal Amount { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Balance allocations stored as JSON
        /// Format: [{"balanceId":1,"percentage":60},{"balanceId":2,"percentage":40}]
        /// These allocations will be applied to all payments created by this cycle
        /// </summary>
        public string? BalanceAllocationsJson { get; set; }

        [JsonIgnore]
        public ICollection<PaymentEntity> Payments { get; set; }

        public CycleEntity()
        {
            Payments = new List<PaymentEntity>();
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}