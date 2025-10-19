using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildingManagement.Constants.Log;

namespace BuildingManagement.Models.Entities
{
    public class AuditEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // UserId is optional - some actions might not have an authenticated user
        public int? UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public LogAction Action { get; set; }

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        [MaxLength(500)]
        public string? OldValues { get; set; }

        [MaxLength(500)]
        public string? NewValues { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public AuditEntity(int? userId, string userEmail, LogAction action, string entityType, int? entityId = null)
        {
            UserId = userId;
            UserEmail = userEmail;
            Action = action;
            EntityType = entityType;
            EntityId = entityId;
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
