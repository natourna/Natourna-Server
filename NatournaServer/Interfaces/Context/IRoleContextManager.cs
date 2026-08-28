using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IRoleContextManager
    {
        Task<RoleEntity?> GetByNameAsync(string name);
    }
}
