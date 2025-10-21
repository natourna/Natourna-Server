namespace BuildingManagement.Models.Api.Response.Building
{
    public class BuildingResponse
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int NumberOfApartments { get; set; }

        public int Floors { get; set; }

        public int ActiveApartments { get; set; }

        public int CompoundId { get; set; }

        public string? CompoundName { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
