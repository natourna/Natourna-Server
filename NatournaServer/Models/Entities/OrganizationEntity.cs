using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatournaServer.Models.Entities
{
    /// <summary>The paying customer (tenant); a single-building customer is an organization whose compound contains exactly one building.</summary>
    public class OrganizationEntity : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>LBP per 1 USD for dual currency display; null means USD-only.</summary>
        public decimal? LbpExchangeRate { get; set; }

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public ICollection<CompoundEntity> Compounds { get; set; }

        [JsonIgnore]
        public ICollection<UserEntity> Users { get; set; }

        [JsonIgnore]
        public SubscriptionEntity? Subscription { get; set; }

        public OrganizationEntity(string name)
        {
            Name = name;
            IsActive = true;
            Compounds = [];
            Users = [];
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
