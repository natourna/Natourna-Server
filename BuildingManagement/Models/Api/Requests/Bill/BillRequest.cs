namespace BuildingManagement.Models.Api.Requests.Bill
{
    public class BillRequest
    {
        public string Label { get; set; }

        public decimal Amount { get; set; }

        public DateTime? DueDate { get; set; }

        public int BalanceId { get; set; }

        public BillRequest(string label, decimal amount, int balanceId)
        {
            Label = label;
            Amount = amount;
            BalanceId = balanceId;
        }
    }
}
