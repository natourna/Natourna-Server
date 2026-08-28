using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IRoleContextManager
    {
        Task<List<RoleEntity>> GetAllAsync();

        Task<RoleEntity?> GetByIdAsync(int id);

        Task<RoleEntity?> GetByNameAsync(string name);

        Task<RoleEntity> CreateAsync(RoleEntity role);
    }
}
