namespace NatournaServer.Models.Api.Response.Organization
{
    public class OrganizationResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal? LbpExchangeRate { get; set; }

        public bool IsActive { get; set; }

        public SubscriptionResponse? Subscription { get; set; }
    }

    public class SubscriptionResponse
    {
        public string Status { get; set; } = string.Empty;

        public decimal PricePerBuilding { get; set; }

        public int BuildingCount { get; set; }

        /// <summary>
        /// PricePerBuilding x BuildingCount - computed, never stored.
        /// </summary>
        public decimal MonthlyCost { get; set; }

        public DateTime StartDate { get; set; }
    }
}
