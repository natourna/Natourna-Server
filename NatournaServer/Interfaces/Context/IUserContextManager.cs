using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IUserContextManager
    {
        Task<List<UserEntity>> GetAllAsync();

        Task<UserEntity?> GetByIdAsync(int id);

        Task<UserEntity?> GetByEmailAsync(string email);

        Task<UserEntity> CreateAsync(UserEntity user);

        Task<UserEntity?> UpdateAsync(int id, UserEntity user);

        Task<bool> DeleteAsync(int id);
    }
}
