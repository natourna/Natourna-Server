using BuildingManagement.Constants.Log;
using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Interfaces.Services;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class UserApiManager : IUserApiManager
    {
        private readonly IUserContextManager _contextManager;
        private readonly IAuditService _auditService;

        public UserApiManager(IUserContextManager contextManager, IAuditService auditService)
        {
            _contextManager = contextManager;
            _auditService = auditService;
        }

        public async Task<List<UserEntity>> GetAllUsersAsync()
        {
            return await _contextManager.GetAllAsync();
        }

        public async Task<UserEntity?> GetUserByIdAsync(int id)
        {
            return await _contextManager.GetByIdAsync(id);
        }

        public async Task<UserEntity> CreateUserAsync(UserEntity user)
        {
            var created = await _contextManager.CreateAsync(user);

            await _auditService.LogAsync(LogAction.Create, "User", created.Id, null, new
            {
                created.Email,
                created.Role,
                created.IsActive
            });

            return created;
        }

        public async Task<UserEntity?> UpdateUserAsync(int id, UserEntity user)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
                return null;

            var oldValues = new
            {
                existing.Email,
                existing.PhoneNumber,
                existing.Role,
                existing.IsActive
            };

            var updated = await _contextManager.UpdateAsync(id, user);

            if (updated != null)
            {
                LogAction action = LogAction.Update;
                if (oldValues.Role != updated.Role)
                {
                    action = LogAction.ChangeRole;
                }

                else if (oldValues.IsActive != updated.IsActive)
                {
                    action = updated.IsActive ? LogAction.ActivateUser : LogAction.DeactivateUser;
                }

                await _auditService.LogAsync(action, "User", id, oldValues, new
                {
                    updated.Email,
                    updated.PhoneNumber,
                    updated.Role,
                    updated.IsActive
                });
            }

            return updated;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _auditService.LogAsync(LogAction.Delete, "User", id, new
            {
                existing.Email,
                existing.Role
            }, null);

            return await _contextManager.DeleteAsync(id);
        }
    }
}
