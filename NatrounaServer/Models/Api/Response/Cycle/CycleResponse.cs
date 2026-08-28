using NatrounaServer.Constants.Cycle;
using NatrounaServer.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NatrounaServer.Models.Api.Response.Cycle
{
    public class CycleResponse
    {
        public int Id { get; set; }

        public string? Label { get; set; }

        public string? Description { get; set; }

        public string? PaymentCycle { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? ApartmentIds { get; set; }

        public decimal Amount { get; set; }

        public bool IsActive { get; set; }

        public string? BalanceAllocations { get; set; }
    }
}
