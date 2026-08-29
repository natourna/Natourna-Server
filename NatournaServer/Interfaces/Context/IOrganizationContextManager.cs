using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IOrganizationContextManager
    {
        Task<OrganizationEntity?> GetByIdAsync(int id);

        Task<OrganizationEntity?> GetFirstAsync();

        Task<OrganizationEntity> CreateAsync(OrganizationEntity organization);

        Task<OrganizationEntity?> UpdateAsync(int id, string name, decimal? lbpExchangeRate);
    }
}
