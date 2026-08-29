using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatournaServer.Models.Entities
{
    public class UserEntity : BaseEntity, ITenantEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Stamped automatically on insert; scoped by a global query filter.
        /// </summary>
        public int OrganizationId { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [JsonIgnore]  // Never expose password in API responses
        public string Password { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        [JsonIgnore]
        public RoleEntity? Role { get; set; }

        public bool IsActive { get; set; } = true;

        public UserEntity(int id, string email, string password, string phoneNumber, int roleId)
        {
            Id = id;
            Email = email;
            Password = password;
            PhoneNumber = phoneNumber;
            RoleId = roleId;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
