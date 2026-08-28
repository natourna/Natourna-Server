namespace NatournaServer.Models.Api.Response.Apartment
{
    public class ApartmentResponse
    {
        public int Id { get; set; }

        public string ApartmentInfo { get; set; } = string.Empty;

        public string? Owner { get; set; }

        public string? Tenant { get; set; }

        public bool? IsActive { get; set; }

        public string Floor { get; set; } = string.Empty;

        public int BuildingId { get; set; }

        public string? BuildingName { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
