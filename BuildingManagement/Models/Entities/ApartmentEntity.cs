using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildingManagement.Const;

namespace BuildingManagement.Models.Entities
{
    public class ApartmentEntity : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string AppartmentNumber { get; set; }

        public string Owner { get; set; }

        public string Tenant { get; set; }

        public ApartmentStatus Status { get; set; }

        public int BuildingId { get; set; }

        [ForeignKey("BuildingId")]
        public BuildingEntity? Building { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public UserEntity? User { get; set; }

        public ICollection<PaymentEntity> Payments { get; set; }

        public ApartmentEntity(int id, string appartmentNumber, string owner, string tenant, ApartmentStatus status, int buildingId, int userId)
        {
            Id = id;
            AppartmentNumber = appartmentNumber;
            Owner = owner;
            Tenant = tenant;
            Status = status;
            BuildingId = buildingId;
            UserId = userId;
            Payments = [];
            CreatedAt = DateTime.UtcNow;
            UpdatededAt = DateTime.UtcNow;
        }
    }
}
