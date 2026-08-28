using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatournaServer.Models.Entities
{
    public class CompoundEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Address { get; set; }

        [JsonIgnore]
        public ICollection<BuildingEntity> Buildings { get; set; }

        [JsonIgnore]
        public ICollection<BalanceEntity> Balances { get; set; }

        public CompoundEntity(string name, string address)
        {
            Name = name;
            Address = address;
            Buildings = [];
            Balances = [];
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
