using NatournaServer.Constants.Log;
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
        private readonly IAuditService _auditService;

        public CompoundApiManager(ICompoundContextManager contextManager, IAuditService auditService)
        {
            _contextManager = contextManager;
            _auditService = auditService;
        }

        public async Task<List<CompoundResponse>> GetAllCompoundsAsync()
        {
            var compounds = await _contextManager.GetAllAsync();
            return compounds.Select(MapToResponse).ToList();
        }

        public async Task<CompoundResponse?> GetCompoundByIdAsync(int id)
        {
            var compound = await _contextManager.GetByIdAsync(id);
            return compound == null ? null : MapToResponse(compound);
        }

        public async Task<CompoundResponse> CreateCompoundAsync(CompoundRequest request)
        {
            var compound = new CompoundEntity(request.Name, request.Address);

            var created = await _contextManager.CreateAsync(compound);

            await _auditService.LogAsync(LogAction.Create, "Compound", created.Id, null, new { created.Name, created.Address });

            return MapToResponse(created);
        }

        public async Task<CompoundResponse?> UpdateCompoundAsync(int id, CompoundRequest request)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            var oldValues = new
            {
                existing.Name,
                existing.Address
            };

            var updated = await _contextManager.UpdateAsync(id, new CompoundEntity(request.Name, request.Address));

            if (updated == null)
            {
                return null;
            }

            await _auditService.LogAsync(LogAction.Update, "Compound", id, oldValues, new { updated.Name, updated.Address });

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteCompoundAsync(int id)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Compound", id, new { existing.Name }, null);

            return await _contextManager.DeleteAsync(id);
        }

        private static CompoundResponse MapToResponse(CompoundEntity compound)
        {
            return new CompoundResponse
            {
                Id = compound.Id,
                Name = compound.Name,
                Address = compound.Address,
                ActiveApartments = compound.Buildings.SelectMany(b => b.Apartments).Count(a => a.IsActive == true),
                CreatedAt = compound.CreatedAt,
                UpdatedAt = compound.UpdatedAt
            };
        }
    }
}
