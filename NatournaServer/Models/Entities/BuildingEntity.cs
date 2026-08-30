using NatournaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatournaServer.Models.Entities
{
    public class BuildingEntity : BaseEntity, ITenantEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>Stamped automatically on insert; scoped by a global query filter.</summary>
        public int OrganizationId { get; set; }

        [Required]
        public string Name { get; set; }

        [RequiredInt]
        public int NumberOfApartments { get; set; }

        [RequiredInt]
        public int Floors { get; set; }

        [RequiredInt]
        public int CompoundId { get; set; }

        [ForeignKey("CompoundId")]
        [JsonIgnore]
        public CompoundEntity? Compound { get; set; }

        [JsonIgnore]
        public ICollection<ApartmentEntity> Apartments { get; set; }

        public BuildingEntity(int id, string name, int numberOfApartments, int floors, int compoundId)
        {
            Id = id;
            Name = name;
            NumberOfApartments = numberOfApartments;
            Floors = floors;
            CompoundId = compoundId;
            Apartments = [];
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
