using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildingManagement.Const;

namespace BuildingManagement.Models.Entities
{
    public class ApartementEntity : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public string AppartementNumber { get; set; }

        public string Owner { get; set; }

        public string Tenant { get; set; }

        public ApartementStatus Status { get; set; }

        public int BuildingId { get; set; }

        [ForeignKey("BuildingId")]
        public BuildingEntity? Building { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public UserEntity? User { get; set; }

        public ICollection<PaymentEntity> Payments { get; set; }

        public ApartementEntity(int id, string appartementNumber, string owner, string tenant, ApartementStatus status, int buildingId, int userId)
        {
            Id = id;
            AppartementNumber = appartementNumber;
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
