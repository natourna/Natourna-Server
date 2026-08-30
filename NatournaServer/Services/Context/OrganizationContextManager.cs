using NatournaServer.Constants.Error;
using NatournaServer.Data;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Services.Context
{
    public class OrganizationContextManager : IOrganizationContextManager
    {
        private readonly NatournaServerContext _context;
        private readonly ILogger<OrganizationContextManager> _logger;

        public OrganizationContextManager(NatournaServerContext context, ILogger<OrganizationContextManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrganizationEntity?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Organization.GetByIdFailed(id);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.ORGANIZATION_GET_BY_ID_ERROR, userMessage);

                throw new ContextException(ErrorCodes.ORGANIZATION_GET_BY_ID_ERROR, userMessage, technicalDetails, ex);
            }
        }

        public async Task<OrganizationEntity?> UpdateAsync(int id, string name, decimal? lbpExchangeRate)
        {
            try
            {
                _logger.LogInformation("Updating organization with ID: {OrganizationId}", id);

                var existingOrganization = await _context.Organizations.FindAsync(id);
                if (existingOrganization == null)
                {
                    _logger.LogWarning("Cannot update - Organization with ID {OrganizationId} not found", id);
                    return null;
                }

                existingOrganization.Name = name;
                existingOrganization.LbpExchangeRate = lbpExchangeRate;
                existingOrganization.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated organization with ID {OrganizationId}", id);

                return existingOrganization;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Organization.UpdateFailed(id);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.ORGANIZATION_UPDATE_ERROR, userMessage);

                throw new ContextException(ErrorCodes.ORGANIZATION_UPDATE_ERROR, userMessage, technicalDetails, ex);
            }
        }
    }
}
