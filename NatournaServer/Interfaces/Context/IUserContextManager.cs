using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IUserContextManager
    {
        Task<bool> AnyAsync();

        Task<List<UserEntity>> GetAllAsync();

        Task<(List<UserEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);

        Task<UserEntity?> GetByIdAsync(int id);

        Task<UserEntity?> GetByEmailAsync(string email);

        Task<UserEntity> CreateAsync(UserEntity user);

        Task<UserEntity?> UpdateAsync(int id, UserEntity user);

        Task<bool> DeleteAsync(int id);
    }
}
