using NatournaServer.Constants.User;
using NatournaServer.Interfaces.Context;
using NatournaServer.Models.Entities;

namespace NatournaServer.Extensions;

public static class SeedExtension
{
    /// <summary>
    /// Ensure the reference roles exist before the application starts serving requests
    /// </summary>
    public static async Task SeedRolesAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleContextManager = scope.ServiceProvider.GetRequiredService<IRoleContextManager>();

        foreach (var name in new[] { RoleNames.User, RoleNames.Admin })
        {
            var existing = await roleContextManager.GetByNameAsync(name);
            if (existing == null)
            {
                await roleContextManager.CreateAsync(new RoleEntity(0, name));
            }
        }
    }
}
