using NatournaServer.Data;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace NatournaServer.Services.Context
{
    public class RoleContextManager : IRoleContextManager
    {
        private readonly NatournaServerContext _context;

        public RoleContextManager(NatournaServerContext context)
        {
            _context = context;
        }

        public async Task<RoleEntity?> GetByNameAsync(string name)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == name);
        }
    }
}
