using NatrounaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatrounaServer.Models.Entities
{
    public class CompoundEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [RequiredInt]
        public int ActiveApartments { get; set; }

        [JsonIgnore]
        public ICollection<BuildingEntity> Buildings { get; set; }

        [JsonIgnore]
        public ICollection<BalanceEntity> Balances { get; set; }

        public CompoundEntity(int id, string name, string address, int activeApartments)
        {
            Id = id;
            Name = name;
            Address = address;
            ActiveApartments = activeApartments;
            Buildings = [];
            Balances = [];
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}