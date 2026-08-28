using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Response.User;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class UserApiManager : IUserApiManager
    {
        private readonly IUserContextManager _contextManager;
        private readonly IRoleContextManager _roleContextManager;
        private readonly IAuditService _auditService;

        public UserApiManager(IUserContextManager contextManager, IRoleContextManager roleContextManager, IAuditService auditService)
        {
            _contextManager = contextManager;
            _roleContextManager = roleContextManager;
            _auditService = auditService;
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _contextManager.GetAllAsync();
            return users.Select(u => MapToResponse(u, u.Role!.Name)).ToList();
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var user = await _contextManager.GetByIdAsync(id);
            return user == null ? null : MapToResponse(user, user.Role!.Name);
        }

        public async Task<UserResponse?> GetUserByEmailAsync(string email)
        {
            var user = await _contextManager.GetByEmailAsync(email);
            return user == null ? null : MapToResponse(user, user.Role!.Name);
        }

        public async Task<UserResponse> CreateUserAsync(UserEntity user)
        {
            var role = await GetRoleOrThrowAsync(user.RoleId, ErrorCodes.USER_CREATE_ERROR);

            var created = await _contextManager.CreateAsync(user);

            await _auditService.LogAsync(LogAction.Create, "User", created.Id, null, new { created.Email, Role = role.Name, created.IsActive });

            return MapToResponse(created, role.Name);
        }

        public async Task<UserResponse?> UpdateUserAsync(int id, UserEntity user)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
                return null;

            var role = await GetRoleOrThrowAsync(user.RoleId, ErrorCodes.USER_UPDATE_ERROR);

            var oldValues = new
            {
                existing.Email,
                existing.PhoneNumber,
                Role = existing.Role!.Name,
                existing.IsActive
            };
            var oldRoleId = existing.RoleId;
            var oldIsActive = existing.IsActive;

            var updated = await _contextManager.UpdateAsync(id, user);
            if (updated == null)
                return null;

            LogAction action = LogAction.Update;
            if (oldRoleId != updated.RoleId)
            {
                action = LogAction.ChangeRole;
            }

            else if (oldIsActive != updated.IsActive)
            {
                action = updated.IsActive ? LogAction.ActivateUser : LogAction.DeactivateUser;
            }

            await _auditService.LogAsync(action, "User", id, oldValues, new { updated.Email, updated.PhoneNumber, Role = role.Name, updated.IsActive });

            return MapToResponse(updated, role.Name);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "User", id, new { existing.Email, Role = existing.Role!.Name }, null);

            return await _contextManager.DeleteAsync(id);
        }

        private async Task<RoleEntity> GetRoleOrThrowAsync(int roleId, string errorCode)
        {
            var role = await _roleContextManager.GetByIdAsync(roleId);
            if (role == null)
            {
                throw new ApiException(errorCode, "The selected role does not exist.", $"No role found with id {roleId}");
            }

            return role;
        }

        private static UserResponse MapToResponse(UserEntity user, string roleName)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                Role = roleName,
                IsActive = user.IsActive
            };
        }
    }
}
