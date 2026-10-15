using EClaim.Application.DTOs.Users;

namespace EClaim.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId, CancellationToken ct = default);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<List<UserListDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task SetUserActiveStatusAsync(int userId, bool isActive, CancellationToken ct = default);
}