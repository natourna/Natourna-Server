using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        public string Password { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [JsonIgnore]
        public ICollection<ApartmentEntity> Apartments { get; set; }

        public UserEntity(int id, string email, string password, string phoneNumber)
        {
            Id = id;
            Email = email;
            Password = password;
            PhoneNumber = phoneNumber;
            Apartments = [];
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
