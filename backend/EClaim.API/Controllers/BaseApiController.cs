using EClaim.Application;
using EClaim.Application.Common;
using EClaim.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EClaim.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public abstract class BaseApiController : ControllerBase
{
    protected ICurrentUserService? _currentUserService;
    protected ICurrentUserService CurrentUserAccessor => _currentUserService ??= HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
    protected CurrentUser CurrentUser => CurrentUserAccessor.GetCurrentUser()
        ?? throw new ForbiddenAccessException("No authenticated user found on the request.");
    protected int CurrentUserId => CurrentUser.UserId;
    protected ActionResult<ApiResponse<T>> Ok200<T>(T data, string? message = null) => Ok(ApiResponse<T>.Ok(data, message));
}