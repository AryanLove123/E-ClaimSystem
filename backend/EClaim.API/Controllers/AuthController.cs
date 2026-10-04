using EClaim.Application.Common;
using EClaim.Application.DTOs.Auth;
using EClaim.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EClaim.API.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        var userId = await _authService.RegisterAsync(request, ct);
        return Ok(ApiResponse<object>.Ok(new { userId }, "Registration successful. Please check your email (or server logs in dev mode) to verify your account."));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken ct)
    {
        await _authService.VerifyEmailAsync(request, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "Email verified successfully. You can now log in."));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
    }
}
