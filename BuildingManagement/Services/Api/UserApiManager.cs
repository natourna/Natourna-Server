using BuildingManagement.Interfaces.Api;
using BuildingManagement.Interfaces.Context;
using BuildingManagement.Models.Entities;

namespace BuildingManagement.Services.Api
{
    public class UserApiManager : IUserApiManager
    {
        private readonly IUserContextManager _contextManager;

        public UserApiManager(IUserContextManager contextManager)
        {
            _contextManager = contextManager;
        }

        public async Task<List<UserEntity>> GetAllUsersAsync()
        {
            return await _contextManager.GetAllAsync();
        }

        public async Task<UserEntity?> GetUserByIdAsync(int id)
        {
            return await _contextManager.GetByIdAsync(id);
        }

        public async Task<UserEntity> CreateUserAsync(UserEntity payment)
        {
            return await _contextManager.CreateAsync(payment);
        }

        public async Task<UserEntity?> UpdateUserAsync(int id, UserEntity payment)
        {
            return await _contextManager.UpdateAsync(id, payment);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _contextManager.DeleteAsync(id);
        }
    }
}
