using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingManagement.Models.Entities
{
    public class BuildingEntity : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int NumberOfApartements { get; set; }

        public int Floors { get; set; }

        public int CompoundId { get; set; }

        [ForeignKey("CompoundId")]
        public CompoundEntity? Compound { get; set; }

        public ICollection<ApartementEntity> Apartments { get; set; }

        public BuildingEntity(int id, string name, int numberOfApartements, int floors, int compoundId)
        {
            Id = id;
            Name = name;
            NumberOfApartements = numberOfApartements;
            Floors = floors;
            CompoundId = compoundId;
            Apartments = [];
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
