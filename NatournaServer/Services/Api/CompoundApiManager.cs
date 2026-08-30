using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Compound;
using NatournaServer.Models.Api.Response.Compound;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class CompoundApiManager : ICompoundApiManager
    {
        private readonly ICompoundContextManager _contextManager;
        private readonly IPaymentContextManager _paymentContextManager;
        private readonly IBillContextManager _billContextManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<CompoundApiManager> _logger;

        public CompoundApiManager(
            ICompoundContextManager contextManager,
            IPaymentContextManager paymentContextManager,
            IBillContextManager billContextManager,
            IAuditService auditService,
            ILogger<CompoundApiManager> logger)
        {
            _contextManager = contextManager;
            _paymentContextManager = paymentContextManager;
            _billContextManager = billContextManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<List<CompoundResponse>> GetAllCompoundsAsync()
        {
            List<CompoundEntity> compounds = await _contextManager.GetAllAsync();
            return compounds.Select(MapToResponse).ToList();
        }

        public async Task<CompoundResponse?> GetCompoundByIdAsync(int id)
        {
            CompoundEntity? compound = await _contextManager.GetByIdAsync(id);
            return compound == null ? null : MapToResponse(compound);
        }

        public async Task<CompoundResponse> CreateCompoundAsync(CompoundRequest compound)
        {
            CompoundEntity created = await _contextManager.CreateAsync(MapToEntity(compound));

            await _auditService.LogAsync(LogAction.Create, "Compound", created.Id, null, new { created.Name, created.Address });

            return MapToResponse(created);
        }

        public async Task<CompoundResponse?> UpdateCompoundAsync(int id, CompoundRequest compound)
        {
            CompoundEntity? existing = await _contextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            var oldValues = new
            {
                existing.Name,
                existing.Address
            };

            CompoundEntity? updated = await _contextManager.UpdateAsync(id, MapToEntity(compound));

            if (updated == null)
            {
                return null;
            }

            await _auditService.LogAsync(LogAction.Update, "Compound", id, oldValues, new { updated.Name, updated.Address });

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteCompoundAsync(int id)
        {
            CompoundEntity? existing = await _contextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return false;
            }

            // The cascade stops at Restrict FKs (payments under apartments, bills under balances);
            // refuse with a clear 409 instead of a raw database error mid-cascade
            if (await _paymentContextManager.AnyAsync(compoundId: id) || await _billContextManager.AnyAsync(compoundId: id))
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Reference.InUse("Compound", id, "payments or bills");
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.COMPOUND_HAS_ACTIVITY_ERROR, userMessage);
                throw new ApiException(ErrorCodes.COMPOUND_HAS_ACTIVITY_ERROR, userMessage, technicalDetails, statusCode: 409);
            }

            await _auditService.LogAsync(LogAction.Delete, "Compound", id, new { existing.Name }, null);

            return await _contextManager.DeleteAsync(id);
        }

        private static CompoundEntity MapToEntity(CompoundRequest request)
        {
            return new CompoundEntity(0, request.Name, request.Address, request.ActiveApartments);
        }

        private static CompoundResponse MapToResponse(CompoundEntity compound)
        {
            return new CompoundResponse
            {
                Id = compound.Id,
                Name = compound.Name,
                Address = compound.Address,
                ActiveApartments = compound.ActiveApartments,
                CreatedAt = compound.CreatedAt,
                UpdatedAt = compound.UpdatedAt
            };
        }
    }
}
