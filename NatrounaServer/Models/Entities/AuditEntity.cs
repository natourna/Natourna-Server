using NatrounaServer.Constants.Log;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NatrounaServer.Models.Entities
{
    public class AuditEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string UserEmail { get; set; }

        [Required]
        public LogAction Action { get; set; }

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; }

        public int? EntityId { get; set; }

        [MaxLength(500)]
        public string? OldValues { get; set; }

        [MaxLength(500)]
        public string? NewValues { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public AuditEntity( string userEmail, LogAction action, string entityType)
        {
            UserEmail = userEmail;
            Action = action;
            EntityType = entityType;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
