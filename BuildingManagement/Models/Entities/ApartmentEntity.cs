using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BuildingManagement.Models.Validation;

namespace BuildingManagement.Models.Entities
{
    public class ApartmentEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ApartmentInfo { get; set; }

        public string? Owner { get; set; }

        public string? Tenant { get; set; }

        [Required]
        public bool? IsActive { get; set; }

        public int Floor { get; set; }

        [RequiredInt]
        public int BuildingId { get; set; }

        [ForeignKey("BuildingId")]
        [JsonIgnore]  // Added back - prevents circular reference
        public BuildingEntity? Building { get; set; }

        [JsonIgnore]
        public ICollection<PaymentEntity> Payments { get; set; }

        public ApartmentEntity(int id, string apartmentInfo, int floor, bool? isActive, int buildingId)
        {
            Id = id;
            ApartmentInfo = apartmentInfo;
            Floor = floor;
            IsActive = isActive;
            BuildingId = buildingId;
            Payments = [];
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
