using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NatournaServer.Models.Entities
{
    public class RoleEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [JsonIgnore]
        public ICollection<UserEntity> Users { get; set; }

        public RoleEntity(int id, string name)
        {
            Id = id;
            Name = name;
            Users = [];
        }
    }
}
