using System.ComponentModel.DataAnnotations;

namespace BuildingManagement.Models.Entities
{
    public class UserEntity : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string PhoneNumber { get; set; }

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
