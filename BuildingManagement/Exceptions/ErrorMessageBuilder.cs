using BuildingManagement.Models.Entities;
using BuildingManagement.Models.Requests.Payment;

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

        public static class Payment
        {
            public static (string userMessage, string technicalDetails) GetAllFailed(
                int? paymentId = null, int? apartmentId = null, int? cycleId = null, bool? isPaid = null)
            {
                return (
                    "Failed to retrieve payments with the specified filters",
                    $"Filters - PaymentId: {paymentId?.ToString() ?? "null"}, " +
                    $"ApartmentId: {apartmentId?.ToString() ?? "null"}, " +
                    $"CycleId: {cycleId?.ToString() ?? "null"}, " +
                    $"IsPaid: {isPaid?.ToString() ?? "null"}"
                );
            }

            public static (string userMessage, string technicalDetails) CreateFailed(PaymentEntity payment)
            {
                return (
                    $"Failed to create payment",
                    $"Amount: {payment.Amount:C}, ApartmentId: {payment.ApartmentId}, " +
                    $"CycleId: {payment.CycleId?.ToString() ?? "null"}, IsPaid: {payment.IsPaid}"
                );
            }

            public static (string userMessage, string technicalDetails) UpdateFailed(int id, PaymentEntity payment)
            {
                return (
                    $"Failed to update payment with ID {id}",
                    $"PaymentId: {id}, New Amount: {payment.Amount:C}, " +
                    $"IsPaid: {payment.IsPaid}"
                );
            }

            public static (string userMessage, string technicalDetails) DeleteFailed(int id)
            {
                return (
                    $"Failed to delete payment with ID {id}",
                    $"PaymentId: {id}"
                );
            }

            public static (string userMessage, string technicalDetails) InvalidBalanceAllocations(decimal totalPercentage)
            {
                return (
                    $"Balance allocations must sum to exactly 100%. Current sum: {totalPercentage}%",
                    $"TotalPercentage: {totalPercentage}%, Expected: 100%"
                );
            }

            public static (string userMessage, string technicalDetails) CreateWithBalancesFailed(PaymentRequest request)
            {
                return (
                    $"Failed to create payment with balance allocations",
                    $"Amount: {request.Amount:C}, ApartmentId: {request.ApartmentId}, " +
                    $"AllocationCount: {request.Allocations?.Count ?? 0}"
                );
            }

            public static (string userMessage, string technicalDetails) MarkAsPaidFailed(int paymentId)
            {
                return (
                    $"Failed to mark payment {paymentId} as paid",
                    $"PaymentId: {paymentId}"
                );
            }

            public static (string userMessage, string technicalDetails) MarkAsUnpaidFailed(int paymentId)
            {
                return (
                    $"Failed to mark payment {paymentId} as unpaid",
                    $"PaymentId: {paymentId}"
                );
            }

            public static (string userMessage, string technicalDetails) PaymentNotFound(int paymentId)
            {
                return (
                    $"Payment with ID {paymentId} was not found",
                    $"PaymentId: {paymentId}"
                );
            }

            public static (string userMessage, string technicalDetails) AlreadyPaid(int paymentId)
            {
                return (
                    $"Payment {paymentId} is already marked as paid",
                    $"PaymentId: {paymentId}, IsPaid: true"
                );
            }

            public static (string userMessage, string technicalDetails) AlreadyUnpaid(int paymentId)
            {
                return (
                    $"Payment {paymentId} is already marked as unpaid",
                    $"PaymentId: {paymentId}, IsPaid: false"
                );
            }
        }

        public static class Cycle
        {
            public static (string userMessage, string technicalDetails) GetAllFailed(int? cycleId = null, bool? isActive = null)
            {
                return (
                    "Failed to retrieve cycles with the specified filters",
                    $"Filters - CycleId: {cycleId?.ToString() ?? "null"}, IsActive: {isActive?.ToString() ?? "null"}"
                );
            }

            public static (string userMessage, string technicalDetails) GetByIdFailed(int id)
            {
                return (
                    $"Failed to retrieve cycle with ID {id}",
                    $"CycleId: {id}"
                );
            }

            public static (string userMessage, string technicalDetails) CreateFailed(CycleEntity cycle)
            {
                return (
                    $"Failed to create cycle '{cycle.Label}'",
                    $"Label: '{cycle.Label}', Cycle: {cycle.Cycle}, Amount: {cycle.Amount:C}, " +
                    $"StartDate: {cycle.StartDate:yyyy-MM-dd}, EndDate: {cycle.EndDate:yyyy-MM-dd}"
                );
            }

            public static (string userMessage, string technicalDetails) UpdateFailed(int id, CycleEntity cycle)
            {
                return (
                    $"Failed to update cycle with ID {id}",
                    $"CycleId: {id}, Label: '{cycle.Label}'"
                );
            }

            public static (string userMessage, string technicalDetails) DeleteFailed(int id)
            {
                return (
                    $"Failed to delete cycle with ID {id}",
                    $"CycleId: {id}"
                );
            }

            public static (string userMessage, string technicalDetails) CycleNotFound(int cycleId)
            {
                return (
                    $"Cycle with ID {cycleId} was not found",
                    $"CycleId: {cycleId}"
                );
            }

            public static (string userMessage, string technicalDetails) InvalidDateRange(DateTime startDate, DateTime endDate)
            {
                return (
                    $"Invalid date range: Start date ({startDate:yyyy-MM-dd}) must be before end date ({endDate:yyyy-MM-dd})",
                    $"StartDate: {startDate:yyyy-MM-dd}, EndDate: {endDate:yyyy-MM-dd}"
                );
            }

            public static (string userMessage, string technicalDetails) InvalidBalanceAllocations(decimal totalPercentage)
            {
                return (
                    $"Balance allocations must sum to exactly 100%. Current sum: {totalPercentage}%",
                    $"TotalPercentage: {totalPercentage}%, Expected: 100%"
                );
            }
        }
    }
}
