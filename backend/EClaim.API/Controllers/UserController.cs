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

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await _userService.GetAllUsersAsync(ct);
        return Ok(ApiResponse<List<UserListDto>>.Ok(users));
    }
}