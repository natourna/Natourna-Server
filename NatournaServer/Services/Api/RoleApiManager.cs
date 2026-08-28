using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Api.Response.Role;

namespace NatournaServer.Services.Api
{
    public class RoleApiManager : IRoleApiManager
    {
        private readonly IRoleContextManager _contextManager;

        public RoleApiManager(IRoleContextManager contextManager)
        {
            _contextManager = contextManager;
        }

        public async Task<List<RoleResponse>> GetAllRolesAsync()
        {
            var roles = await _contextManager.GetAllAsync();
            return roles.Select(r => new RoleResponse { Id = r.Id, Name = r.Name }).ToList();
        }
    }
}
