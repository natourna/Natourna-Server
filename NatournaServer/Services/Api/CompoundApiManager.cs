using NatournaServer.Constants.Log;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Compound;
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

        public async Task<List<CompoundEntity>> GetAllCompoundsAsync()
        {
            return await _contextManager.GetAllAsync();
        }

        public async Task<CompoundEntity?> GetCompoundByIdAsync(int id)
        {
            return await _contextManager.GetByIdAsync(id);
        }

        public async Task<CompoundEntity> CreateCompoundAsync(CompoundRequest compound)
        {
            var created = await _contextManager.CreateAsync(MapToEntity(compound));

            await _auditService.LogAsync(LogAction.Create, "Compound", created.Id, null, new { created.Name, created.Address });

            return created;
        }

        public async Task<CompoundEntity?> UpdateCompoundAsync(int id, CompoundRequest compound)
        {
            var existing = await GetCompoundByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            var oldValues = new
            {
                existing.Name,
                existing.Address
            };

            var updated = await _contextManager.UpdateAsync(id, MapToEntity(compound));

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Compound", id, oldValues, new { updated.Name, updated.Address });
            }

            return updated;
        }

        private static CompoundEntity MapToEntity(CompoundRequest request)
        {
            return new CompoundEntity(0, request.Name, request.Address, request.ActiveApartments);
        }

        public async Task<bool> DeleteCompoundAsync(int id)
        {
            var existing = await GetCompoundByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "Compound", id, new { existing.Name }, null);

            return await _contextManager.DeleteAsync(id);
        }
    }
}