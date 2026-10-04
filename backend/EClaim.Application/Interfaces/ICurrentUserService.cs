using EClaim.Application.Common;

namespace EClaim.Application;

public interface ICurrentUserService
{
    CurrentUser? GetCurrentUser();
}
