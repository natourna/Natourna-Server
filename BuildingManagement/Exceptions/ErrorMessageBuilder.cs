using BuildingManagement.Models.Entities;

namespace BuildingManagement.Exceptions
{
    /// <summary>
    /// Helper class for building consistent error messages and technical details for ContextException
    /// </summary>
    public static class ErrorMessageBuilder
    {
        public static class Bill
        {
            public static (string userMessage, string technicalDetails) GetAllFailed(
                int? balanceId, bool? isPaid, DateTime? dueDateFrom, DateTime? dueDateTo)
            {
                return (
                    "Failed to retrieve bills with the specified filters",
                    $"Filters - BalanceId: {balanceId?.ToString() ?? "null"}, " +
                    $"IsPaid: {isPaid?.ToString() ?? "null"}, " +
                    $"DueDateFrom: {dueDateFrom?.ToString("yyyy-MM-dd") ?? "null"}, " +
                    $"DueDateTo: {dueDateTo?.ToString("yyyy-MM-dd") ?? "null"}"
                );
            }

            public static (string userMessage, string technicalDetails) GetByIdFailed(int id)
            {
                return (
                    $"Failed to retrieve bill with ID {id}",
                    $"BillId: {id}"
                );
            }

            public static (string userMessage, string technicalDetails) GetByBalanceIdFailed(int balanceId)
            {
                return (
                    $"Failed to retrieve bills for balance with ID {balanceId}",
                    $"BalanceId: {balanceId}"
                );
            }

            public static (string userMessage, string technicalDetails) CreateFailed(BillEntity bill)
            {
                return (
                    $"Failed to create bill '{bill.Label}'",
                    $"Label: '{bill.Label}', Amount: {bill.Amount:C}, " +
                    $"BalanceId: {bill.BalanceId}, DueDate: {bill.DueDate?.ToString("yyyy-MM-dd") ?? "null"}, " +
                    $"IsPaid: {bill.IsPaid}"
                );
            }

            public static (string userMessage, string technicalDetails) UpdateFailed(int id, BillEntity bill)
            {
                return (
                    $"Failed to update bill with ID {id}",
                    $"BillId: {id}, New Amount: {bill.Amount:C}, " +
                    $"New DueDate: {bill.DueDate?.ToString("yyyy-MM-dd") ?? "null"}"
                );
            }

            public static (string userMessage, string technicalDetails) DeleteFailed(int id)
            {
                return (
                    $"Failed to delete bill with ID {id}",
                    $"BillId: {id}"
                );
            }

            public static (string userMessage, string technicalDetails) MarkAsPaidFailed(int billId)
            {
                return (
                    $"Failed to mark bill {billId} as paid",
                    $"BillId: {billId}"
                );
            }

            public static (string userMessage, string technicalDetails) MarkAsUnpaidFailed(int billId)
            {
                return (
                    $"Failed to mark bill {billId} as unpaid",
                    $"BillId: {billId}"
                );
            }

            public static (string userMessage, string technicalDetails) BillNotFound(int billId)
            {
                return (
                    $"Bill with ID {billId} was not found",
                    $"BillId: {billId}"
                );
            }

            public static (string userMessage, string technicalDetails) InsufficientBalance(
                int billId, int balanceId, decimal billAmount, decimal currentBalance)
            {
                return (
                    $"Insufficient balance to pay bill {billId}. Required: {billAmount:C}, Available: {currentBalance:C}",
                    $"BillId: {billId}, BalanceId: {balanceId}, BillAmount: {billAmount:C}, " +
                    $"CurrentBalance: {currentBalance:C}, Shortage: {(billAmount - currentBalance):C}"
                );
            }

            public static (string userMessage, string technicalDetails) AlreadyPaid(int billId)
            {
                return (
                    $"Bill {billId} is already marked as paid",
                    $"BillId: {billId}, IsPaid: true"
                );
            }

            public static (string userMessage, string technicalDetails) AlreadyUnpaid(int billId)
            {
                return (
                    $"Bill {billId} is already marked as unpaid",
                    $"BillId: {billId}, IsPaid: false"
                );
            }
        }

        public static class Balance
        {
            public static (string userMessage, string technicalDetails) GetAllFailed(int? balanceId = null, int? compoundId = null)
            {
                return (
                    "Failed to retrieve balances with the specified filters",
                    $"Filters - BalanceId: {balanceId?.ToString() ?? "null"}, " +
                    $"CompoundId: {compoundId?.ToString() ?? "null"}"
                );
            }

            public static (string userMessage, string technicalDetails) GetByIdFailed(int id)
            {
                return (
                    $"Failed to retrieve balance with ID {id}",
                    $"BalanceId: {id}"
                );
            }

            public static (string userMessage, string technicalDetails) GetByCompoundIdFailed(int compoundId)
            {
                return (
                    $"Failed to retrieve balances for compound with ID {compoundId}",
                    $"CompoundId: {compoundId}"
                );
            }

            public static (string userMessage, string technicalDetails) CreateFailed(BalanceEntity balance)
            {
                return (
                    $"Failed to create balance '{balance.Label}'",
                    $"Label: '{balance.Label}', CurrentAmount: {balance.CurrentAmount:C}, CompoundId: {balance.CompoundId}"
                );
            }

            public static (string userMessage, string technicalDetails) UpdateFailed(int id, BalanceEntity balance)
            {
                return (
                    $"Failed to update balance with ID {id}",
                    $"BalanceId: {id}, New Label: '{balance.Label}'"
                );
            }

            public static (string userMessage, string technicalDetails) DeleteFailed(int id)
            {
                return (
                    $"Failed to delete balance with ID {id}",
                    $"BalanceId: {id}"
                );
            }

            public static (string userMessage, string technicalDetails) NotFound(int balanceId)
            {
                return (
                    $"Balance with ID {balanceId} was not found",
                    $"BalanceId: {balanceId}"
                );
            }
        }
    }
}
