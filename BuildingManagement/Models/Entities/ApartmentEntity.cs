using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BuildingManagement.Const;
using BuildingManagement.Validation;

namespace BuildingManagement.Models.Entities
{
    public class ApartmentEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ApartmentInfo { get; set; }

        [Required]
        public string Owner { get; set; }

        public string? Tenant { get; set; }

        [Required]
        public bool? IsActive { get; set; }

        [RequiredInt]
        public int Floor { get; set; }

        [RequiredInt]
        public int BuildingId { get; set; }

        [ForeignKey("BuildingId")]
        [JsonIgnore]
        public BuildingEntity? Building { get; set; }

        [RequiredInt]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public UserEntity? User { get; set; }

        [JsonIgnore]
        public ICollection<PaymentEntity> Payments { get; set; }

        public ApartmentEntity(int id, string apartmentInfo, string owner, int floor, bool? isActive, int buildingId, int userId)
        {
            Id = id;
            ApartmentInfo = apartmentInfo;
            Owner = owner;
            Floor = floor;
            IsActive = isActive;
            BuildingId = buildingId;
            UserId = userId;
            Payments = [];
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
