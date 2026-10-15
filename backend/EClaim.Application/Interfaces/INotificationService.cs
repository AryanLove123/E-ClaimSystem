using EClaim.Domain.Entities;

namespace EClaim.Application.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(User user, string subject, string message, int? claimId = null, CancellationToken ct = default);
    Task<NotificationSummaryDto> GetSummaryAsync(int userId, int take = 10, CancellationToken ct = default);
    Task MarkAsReadAsync(int userId, int notificationId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(int userId, CancellationToken ct = default);
}