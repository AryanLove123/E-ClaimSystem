using System.Security.Claims;
using EClaim.Application;
using EClaim.Application.Common;
using Microsoft.AspNetCore.Http;

namespace EClaim.Infrastructure.Authentication;
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser? GetCurrentUser()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true) return null;

        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (idClaim is null || !int.TryParse(idClaim, out var userId)) return null;

        return new CurrentUser
        {
            UserId = userId,
            Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
            Role = principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty
        };
    }
}

