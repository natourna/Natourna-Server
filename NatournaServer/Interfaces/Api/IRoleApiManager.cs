using NatournaServer.Models.Api.Response.Role;

namespace NatournaServer.Interfaces.Api
{
    public interface IRoleApiManager
    {
        Task<List<RoleResponse>> GetAllRolesAsync();
    }
}
