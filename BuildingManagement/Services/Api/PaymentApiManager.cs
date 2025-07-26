using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class PaymentApiManager : IPaymentApiManager
    {
        private readonly IPaymentContextManager _contextManager;

        public PaymentApiManager(IPaymentContextManager contextManager)
        {
            _contextManager = contextManager;
        }

        public async Task<List<PaymentEntity>> GetAllPaymentsAsync()
        {
            return await _contextManager.GetAllAsync();
        }

        public async Task<PaymentEntity?> GetPaymentByIdAsync(int id)
        {
            return await _contextManager.GetByIdAsync(id);
        }

        public async Task<List<PaymentEntity>> GetPaymentsByBillIdAsync(int billId)
        {
            return await _contextManager.GetByBillIdAsync(billId);
        }

        public async Task<List<PaymentEntity>> GetPaymentsByApartmentIdAsync(int apartmentId)
        {
            return await _contextManager.GetByApartmentIdAsync(apartmentId);
        }

        public async Task<PaymentEntity> CreatePaymentAsync(PaymentEntity payment)
        {
            return await _contextManager.CreateAsync(payment);
        }

        public async Task<PaymentEntity?> UpdatePaymentAsync(int id, PaymentEntity payment)
        {
            return await _contextManager.UpdateAsync(id, payment);
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            return await _contextManager.DeleteAsync(id);
        }
    }
}