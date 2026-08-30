using NatournaServer.Constants.Error;
using NatournaServer.Data;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Services.Context
{
    public class SubscriptionContextManager : ISubscriptionContextManager
    {
        private readonly NatournaServerContext _context;
        private readonly ILogger<SubscriptionContextManager> _logger;

        public SubscriptionContextManager(NatournaServerContext context, ILogger<SubscriptionContextManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SubscriptionEntity?> GetByOrganizationIdAsync(int organizationId)
        {
            try
            {
                return await _context.Subscriptions.FirstOrDefaultAsync(s => s.OrganizationId == organizationId);
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Organization.SubscriptionGetFailed(organizationId);

                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.SUBSCRIPTION_GET_ERROR, userMessage);

                throw new ContextException(ErrorCodes.SUBSCRIPTION_GET_ERROR, userMessage, technicalDetails, ex);
            }
        }
    }
}
