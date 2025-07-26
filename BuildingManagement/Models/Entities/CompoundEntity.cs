using System.ComponentModel.DataAnnotations;

namespace BuildingManagement.Models.Entities
{
    public class CompoundEntity : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public int ActiveApartements { get; set; }

        public ICollection<BuildingEntity> Buildings { get; set; }

        public ICollection<BillEntity> Bills { get; set; }

        public CompoundEntity(int id, string name, string address, int activeApartements)
        {
            Id = id;
            Name = name;
            Address = address;
            ActiveApartements = activeApartements;
            Buildings = [];
            Bills = [];
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}