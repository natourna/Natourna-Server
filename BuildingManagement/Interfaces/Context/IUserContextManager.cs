using BuildingManagement.Models.Entities;

namespace BuildingManagement.Interfaces.Context
{
    public interface IUserContextManager
    {
        Task<List<UserEntity>> GetAllAsync();

        Task<UserEntity?> GetByIdAsync(int id);

        Task<UserEntity> CreateAsync(UserEntity payment);

        Task<UserEntity?> UpdateAsync(int id, UserEntity payment);

        Task<bool> DeleteAsync(int id);
    }
}
