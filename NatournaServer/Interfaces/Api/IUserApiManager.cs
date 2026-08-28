using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Requests.User;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Api.Response.User;

namespace NatournaServer.Interfaces.Api
{
    public interface IUserApiManager
    {
        Task<PagedResponse<UserResponse>> GetPagedUsersAsync(PagedQuery query);

        Task<UserResponse?> GetUserByIdAsync(int id);

        Task<UserResponse?> GetUserByEmailAsync(string email);

        Task<UserResponse> CreateUserAsync(CreateUserRequest user);

        Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest user);

        Task<bool> DeleteUserAsync(int id);
    }
}
