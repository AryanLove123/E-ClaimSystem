using EClaim.Application.DTOs.Users;

namespace EClaim.Application.Interfaces;

public interface IUserService
{
    Task<List<UserListDto>> GetAllUsersAsync(CancellationToken ct = default);
}