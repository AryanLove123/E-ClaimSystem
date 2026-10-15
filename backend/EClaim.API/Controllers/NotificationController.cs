using EClaim.Application.Common;
using EClaim.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EClaim.API.Controllers;

[Authorize]

public class NotificationController : BaseApiController
{
    private INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary([FromQuery] int take = 10, CancellationToken ct = default)
    {
        var summary = await _notificationService.GetSummaryAsync(CurrentUserId, Math.Clamp(take, 1, 20), ct);
        return Ok(ApiResponse<NotificationSummaryDto>.Ok(summary));
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
    {
        await _notificationService.MarkAsReadAsync(CurrentUserId, id, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "Notification marked as read."));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await _notificationService.MarkAllAsReadAsync(CurrentUserId, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "All notifications marked as read."));
    }
}