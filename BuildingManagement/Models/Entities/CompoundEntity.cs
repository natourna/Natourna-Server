using System.ComponentModel.DataAnnotations;

namespace BuildingManagement.Models.Entities
{
    public class CompoundEntity : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public int ActiveApartments { get; set; }

        public ICollection<BuildingEntity> Buildings { get; set; }

        public ICollection<BillEntity> Bills { get; set; }

        public CompoundEntity(int id, string name, string address, int activeApartments)
        {
            Id = id;
            Name = name;
            Address = address;
            ActiveApartments = activeApartments;
            Buildings = [];
            Bills = [];
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}