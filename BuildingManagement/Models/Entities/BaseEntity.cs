namespace BuildingManagement.Models.Entities
{
    public class BaseEntity
    {
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
