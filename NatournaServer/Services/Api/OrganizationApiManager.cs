using NatournaServer.Constants.Log;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Interfaces.Tenancy;
using NatournaServer.Models.Api.Requests.Organization;
using NatournaServer.Models.Api.Response.Organization;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class OrganizationApiManager : IOrganizationApiManager
    {
        private readonly IOrganizationContextManager _organizationContextManager;
        private readonly ISubscriptionContextManager _subscriptionContextManager;
        private readonly IBuildingContextManager _buildingContextManager;
        private readonly IAuditService _auditService;
        private readonly ITenantContext _tenantContext;

        public OrganizationApiManager(
            IOrganizationContextManager organizationContextManager,
            ISubscriptionContextManager subscriptionContextManager,
            IBuildingContextManager buildingContextManager,
            IAuditService auditService,
            ITenantContext tenantContext)
        {
            _organizationContextManager = organizationContextManager;
            _subscriptionContextManager = subscriptionContextManager;
            _buildingContextManager = buildingContextManager;
            _auditService = auditService;
            _tenantContext = tenantContext;
        }

        public async Task<OrganizationResponse?> GetMyOrganizationAsync()
        {
            int? organizationId = _tenantContext.OrganizationId;

            if (organizationId == null)
            {
                return null;
            }

            OrganizationEntity? organization = await _organizationContextManager.GetByIdAsync(organizationId.Value);

            if (organization == null)
            {
                return null;
            }

            return await MapToResponseAsync(organization);
        }

        public async Task<OrganizationResponse?> UpdateSettingsAsync(UpdateOrganizationSettingsRequest request)
        {
            int? organizationId = _tenantContext.OrganizationId;

            if (organizationId == null)
            {
                return null;
            }

            OrganizationEntity? existing = await _organizationContextManager.GetByIdAsync(organizationId.Value);

            if (existing == null)
            {
                return null;
            }

            var oldValues = new { existing.Name, existing.LbpExchangeRate };

            OrganizationEntity? updated = await _organizationContextManager.UpdateAsync(organizationId.Value, request.Name, request.LbpExchangeRate);

            if (updated == null)
            {
                return null;
            }

            await _auditService.LogAsync(LogAction.Update, "Organization", updated.Id, oldValues, new { updated.Name, updated.LbpExchangeRate });

            return await MapToResponseAsync(updated);
        }

        private async Task<OrganizationResponse> MapToResponseAsync(OrganizationEntity organization)
        {
            SubscriptionEntity? subscription = await _subscriptionContextManager.GetByOrganizationIdAsync(organization.Id);

            SubscriptionResponse? subscriptionResponse = null;

            if (subscription != null)
            {
                // Buildings are tenant-scoped by the global query filter, so this counts the caller's org only
                List<BuildingEntity> buildings = await _buildingContextManager.GetAllAsync();

                subscriptionResponse = new SubscriptionResponse
                {
                    Status = subscription.Status.ToString(),
                    PricePerBuilding = subscription.PricePerBuilding,
                    BuildingCount = buildings.Count,
                    MonthlyCost = subscription.PricePerBuilding * buildings.Count,
                    StartDate = subscription.StartDate
                };
            }

            return new OrganizationResponse
            {
                Id = organization.Id,
                Name = organization.Name,
                LbpExchangeRate = organization.LbpExchangeRate,
                IsActive = organization.IsActive,
                Subscription = subscriptionResponse
            };
        }
    }
}
