using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.User;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Api.Response.User;
using NatournaServer.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace NatournaServer.Services.Api
{
    public class UserApiManager : IUserApiManager
    {
        private readonly IUserContextManager _contextManager;
        private readonly IRoleContextManager _roleContextManager;
        private readonly IPasswordHasher<UserEntity> _passwordHasher;
        private readonly IAuditService _auditService;

        public UserApiManager(IUserContextManager contextManager, IRoleContextManager roleContextManager, IPasswordHasher<UserEntity> passwordHasher, IAuditService auditService)
        {
            _contextManager = contextManager;
            _roleContextManager = roleContextManager;
            _passwordHasher = passwordHasher;
            _auditService = auditService;
        }

        public async Task<PagedResponse<UserResponse>> GetUsersAsync(int page, int pageSize, string? search)
        {
            (List<UserEntity> items, int totalCount) = await _contextManager.GetPagedAsync(page, pageSize, search);

            return new PagedResponse<UserResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var user = await _contextManager.GetByIdAsync(id);
            return user == null ? null : MapToResponse(user);
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            var role = await ResolveRoleAsync(request.Role);

            var existing = await _contextManager.GetByEmailAsync(request.Email);
            if (existing != null)
            {
                throw new ApiException(ErrorCodes.USER_EMAIL_EXISTS_ERROR, "A user with this email already exists", $"Email: {request.Email}");
            }

            var user = new UserEntity(request.Email, string.Empty, request.PhoneNumber, role.Id);
            user.Password = _passwordHasher.HashPassword(user, request.Password);

            var created = await _contextManager.CreateAsync(user);

            await _auditService.LogAsync(LogAction.Create, "User", created.Id, null, new { created.Email, Role = role.Name, created.IsActive });

            return MapToResponse(created);
        }

        public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
                return null;

            var role = await ResolveRoleAsync(request.Role);

            var duplicate = await _contextManager.GetByEmailAsync(request.Email);
            if (duplicate != null && duplicate.Id != id)
            {
                throw new ApiException(ErrorCodes.USER_EMAIL_EXISTS_ERROR, "A user with this email already exists", $"Email: {request.Email}");
            }

            var oldValues = new
            {
                existing.Email,
                existing.PhoneNumber,
                Role = existing.Role?.Name,
                existing.IsActive
            };

            string? passwordHash = string.IsNullOrEmpty(request.Password)
                ? null
                : _passwordHasher.HashPassword(existing, request.Password);

            var updated = await _contextManager.UpdateAsync(id, request.Email, request.PhoneNumber, role.Id, request.IsActive, passwordHash);

            if (updated == null)
                return null;

            LogAction action = LogAction.Update;
            if (oldValues.Role != updated.Role?.Name)
            {
                action = LogAction.ChangeRole;
            }
            else if (oldValues.IsActive != updated.IsActive)
            {
                action = updated.IsActive ? LogAction.ActivateUser : LogAction.DeactivateUser;
            }

            await _auditService.LogAsync(action, "User", id, oldValues, new { updated.Email, updated.PhoneNumber, Role = updated.Role?.Name, updated.IsActive });

            return MapToResponse(updated);
        }

        public async Task<UserResponse?> SetUserActiveAsync(int id, bool isActive)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
                return null;

            var updated = await _contextManager.SetActiveAsync(id, isActive);
            if (updated == null)
                return null;

            LogAction action = isActive ? LogAction.ActivateUser : LogAction.DeactivateUser;
            await _auditService.LogAsync(action, "User", id, new { existing.IsActive }, new { IsActive = isActive });

            return MapToResponse(updated);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "User", id, new { existing.Email, Role = existing.Role?.Name }, null);

            return await _contextManager.DeleteAsync(id);
        }

        private async Task<RoleEntity> ResolveRoleAsync(string roleName)
        {
            var role = await _roleContextManager.GetByNameAsync(roleName);
            if (role == null)
            {
                throw new ApiException(ErrorCodes.USER_ROLE_INVALID_ERROR, "The requested role does not exist", $"Role: {roleName}");
            }

            return role;
        }

        private static UserResponse MapToResponse(UserEntity user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role?.Name ?? string.Empty,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
