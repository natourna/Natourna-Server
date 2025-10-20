using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BuildingManagement.Constants.User;

namespace BuildingManagement.Models.Entities
{
    public class UserEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [JsonIgnore]  // Never expose password in API responses
        public string Password { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = true;

        public UserEntity(int id, string email, string password, string phoneNumber, UserRole role = UserRole.User)
        {
            Id = id;
            Email = email;
            Password = password;
            PhoneNumber = phoneNumber;
            Role = role;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
