using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface ISubscriptionContextManager
    {
        Task<SubscriptionEntity?> GetByOrganizationIdAsync(int organizationId);
    }
}
