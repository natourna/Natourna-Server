using NatournaServer.Models.Api.Response.User;
using NatournaServer.Models.Entities;

namespace NatournaServer.Interfaces.Api
{
    public interface IUserApiManager
    {
        Task<List<UserResponse>> GetAllUsersAsync();

        Task<UserResponse?> GetUserByIdAsync(int id);

        Task<UserResponse?> GetUserByEmailAsync(string email);

        Task<UserResponse> CreateUserAsync(UserEntity user);

        Task<UserResponse?> UpdateUserAsync(int id, UserEntity user);

        Task<bool> DeleteUserAsync(int id);
    }
}
