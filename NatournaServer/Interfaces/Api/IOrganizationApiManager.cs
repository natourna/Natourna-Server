using NatournaServer.Models.Api.Requests.Organization;
using NatournaServer.Models.Api.Response.Organization;

namespace NatournaServer.Interfaces.Api
{
    public interface IOrganizationApiManager
    {
        Task<OrganizationResponse?> GetMyOrganizationAsync();

        Task<OrganizationResponse?> UpdateSettingsAsync(UpdateOrganizationSettingsRequest request);
    }
}
