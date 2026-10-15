using EClaim.Application.Common;
using EClaim.Application.DTOs.Users;
using EClaim.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EClaim.API.Controllers;

[Authorize]

public class UsersController : BaseApiController
{
    private IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var profile = await _userService.GetProfileAsync(CurrentUserId, ct);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request, CancellationToken ct)
    {
        var profile = await _userService.UpdateProfileAsync(CurrentUserId, request, ct);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile, "Profile updated."));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await _userService.GetAllUsersAsync(ct);
        return Ok(ApiResponse<List<UserListDto>>.Ok(users));
    }
}