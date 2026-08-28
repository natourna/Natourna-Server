using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Context
{
    public interface IUserContextManager
    {
        Task<(List<UserEntity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null);

        Task<UserEntity?> GetByIdAsync(int id);

        Task<UserEntity?> GetByEmailAsync(string email);

        Task<UserEntity> CreateAsync(UserEntity user);

        Task<UserEntity?> UpdateAsync(int id, string email, string phoneNumber, int roleId, bool isActive, string? passwordHash);

        Task<UserEntity?> SetActiveAsync(int id, bool isActive);

        Task UpdatePasswordHashAsync(int id, string passwordHash);

        Task<bool> DeleteAsync(int id);
    }
}
