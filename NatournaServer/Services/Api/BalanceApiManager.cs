using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Balance;
using NatournaServer.Models.Api.Response.Balance;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class BalanceApiManager : IBalanceApiManager
    {
        private readonly IBalanceContextManager _balanceContextManager;
        private readonly ICompoundContextManager _compoundContextManager;
        private readonly IBillContextManager _billContextManager;
        private readonly IPaymentAllocationContextManager _paymentAllocationContextManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<BalanceApiManager> _logger;

        public BalanceApiManager(
            IBalanceContextManager balanceContextManager,
            ICompoundContextManager compoundContextManager,
            IBillContextManager billContextManager,
            IPaymentAllocationContextManager paymentAllocationContextManager,
            IAuditService auditService,
            ILogger<BalanceApiManager> logger)
        {
            _balanceContextManager = balanceContextManager;
            _compoundContextManager = compoundContextManager;
            _billContextManager = billContextManager;
            _paymentAllocationContextManager = paymentAllocationContextManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<List<BalanceResponse>> GetAllBalancesAsync()
        {
            List<BalanceEntity> balances = await _balanceContextManager.GetAllAsync();
            return balances.Select(MapToResponse).ToList();
        }

        public async Task<BalanceResponse?> GetBalanceByIdAsync(int id)
        {
            BalanceEntity? balance = await _balanceContextManager.GetByIdAsync(id);
            return balance == null ? null : MapToResponse(balance);
        }

        public async Task<List<BalanceResponse>> GetBalancesByCompoundIdAsync(int compoundId)
        {
            List<BalanceEntity> balances = await _balanceContextManager.GetAllAsync(compoundId: compoundId);
            return balances.Select(MapToResponse).ToList();
        }

        public async Task<BalanceResponse> CreateBalanceAsync(BalanceRequest balance)
        {
            await EnsureCompoundExistsAsync(balance.CompoundId);

            BalanceEntity created = await _balanceContextManager.CreateAsync(MapToEntity(balance));

            await _auditService.LogAsync(LogAction.Create, "Balance", created.Id, null, new { created.CompoundId, created.CurrentAmount, created.Label });

            return MapToResponse(created);
        }

        public async Task<BalanceResponse?> UpdateBalanceAsync(int id, BalanceRequest balance)
        {
            BalanceEntity? existing = await _balanceContextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            await EnsureCompoundExistsAsync(balance.CompoundId);

            var oldValues = new
            {
                existing.CompoundId,
                existing.CurrentAmount,
                existing.Label
            };

            BalanceEntity? updated = await _balanceContextManager.UpdateAsync(id, MapToEntity(balance));

            if (updated == null)
            {
                return null;
            }

            await _auditService.LogAsync(LogAction.Update, "Balance", id, oldValues, new { updated.CompoundId, updated.CurrentAmount, updated.Label });

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteBalanceAsync(int id)
        {
            BalanceEntity? existing = await _balanceContextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return false;
            }

            // Balance->Bills and Balance->PaymentAllocations FKs are Restrict; fail with a clear 409
            if (await _billContextManager.AnyAsync(balanceId: id) || await _paymentAllocationContextManager.AnyAsync(balanceId: id))
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Reference.InUse("Balance", id, "bills or payment allocations");
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BALANCE_IN_USE_ERROR, userMessage);
                throw new ApiException(ErrorCodes.BALANCE_IN_USE_ERROR, userMessage, technicalDetails, statusCode: 409);
            }

            await _auditService.LogAsync(LogAction.Delete, "Balance", id, new { existing.CompoundId, existing.CurrentAmount }, null);

            return await _balanceContextManager.DeleteAsync(id);
        }

        private async Task EnsureCompoundExistsAsync(int compoundId)
        {
            CompoundEntity? compound = await _compoundContextManager.GetByIdAsync(compoundId);

            if (compound == null)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Reference.NotFound("Compound", compoundId);
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.COMPOUND_NOT_FOUND_ERROR, userMessage);
                throw new ApiException(ErrorCodes.COMPOUND_NOT_FOUND_ERROR, userMessage, technicalDetails, statusCode: 404);
            }
        }

        private static BalanceEntity MapToEntity(BalanceRequest request)
        {
            return new BalanceEntity(request.Label, request.CompoundId)
            {
                CurrentAmount = request.CurrentAmount
            };
        }

        private static BalanceResponse MapToResponse(BalanceEntity balance)
        {
            return new BalanceResponse
            {
                Id = balance.Id,
                Label = balance.Label,
                CurrentAmount = balance.CurrentAmount,
                CompoundId = balance.CompoundId,
                CreatedAt = balance.CreatedAt,
                UpdatedAt = balance.UpdatedAt
            };
        }
    }
}
