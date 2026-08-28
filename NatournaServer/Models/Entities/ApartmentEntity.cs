using NatournaServer.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatournaServer.Models.Entities
{
    public class ApartmentEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ApartmentInfo { get; set; }

        [MaxLength(255)]
        public string? Owner { get; set; }

        [MaxLength(255)]
        public string? Tenant { get; set; }

        [Required]
        public bool? IsActive { get; set; }

        [MaxLength(50)]
        public string Floor { get; set; }

        [RequiredInt]
        public int BuildingId { get; set; }

        [ForeignKey("BuildingId")]
        [JsonIgnore]
        public BuildingEntity? Building { get; set; }

        [JsonIgnore]
        public ICollection<PaymentEntity> Payments { get; set; }

        public ApartmentEntity(string apartmentInfo, string floor, bool? isActive, int buildingId)
        {
            ApartmentInfo = apartmentInfo;
            Floor = floor;
            IsActive = isActive;
            BuildingId = buildingId;
            Payments = [];
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
