using NatournaServer.Models.Api.Requests.User;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Api.Response.User;

namespace NatournaServer.Interfaces.Api
{
    public interface IUserApiManager
    {
        Task<PagedResponse<UserResponse>> GetUsersAsync(int page, int pageSize, string? search);

        Task<UserResponse?> GetUserByIdAsync(int id);

        Task<UserResponse> CreateUserAsync(CreateUserRequest request);

        Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request);

        Task<UserResponse?> SetUserActiveAsync(int id, bool isActive);

        Task<bool> DeleteUserAsync(int id);
    }
}
