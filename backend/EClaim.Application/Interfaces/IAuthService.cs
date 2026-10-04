using EClaim.Application.DTOs.Auth;

namespace EClaim.Application.Interfaces;

public interface IAuthService
{
    Task<int> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
