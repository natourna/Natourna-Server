using System.ComponentModel.DataAnnotations;

namespace BuildingManagement.Models.Api.Response.Cycle
{
    public class CycleAllocationResponse
    {
        public int BalanceId { get; set; }

        public string? BalanceName { get; set; }

        public decimal Percentage { get; set; }
    }
}
