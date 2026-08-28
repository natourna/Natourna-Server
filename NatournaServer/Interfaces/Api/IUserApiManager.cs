using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Api
{
    public interface IUserApiManager
    {
        Task<List<UserEntity>> GetAllUsersAsync();

        Task<UserEntity?> GetUserByIdAsync(int id);

        Task<UserEntity> CreateUserAsync(UserEntity user);

        Task<UserEntity?> UpdateUserAsync(int id, UserEntity user);

        Task<bool> DeleteUserAsync(int id);
    }
}
