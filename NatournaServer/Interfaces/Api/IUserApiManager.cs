using NatournaServer.Models.Api.Requests.User;
using NatournaServer.Models.Api.Response.User;

namespace NatournaServer.Interfaces.Api
{
    public interface IUserApiManager
    {
        Task<List<UserResponse>> GetAllUsersAsync();

        Task<UserResponse?> GetUserByIdAsync(int id);

        Task<UserResponse?> GetUserByEmailAsync(string email);

        Task<UserResponse> CreateUserAsync(CreateUserRequest user);

        Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest user);

        Task<bool> DeleteUserAsync(int id);
    }
}
