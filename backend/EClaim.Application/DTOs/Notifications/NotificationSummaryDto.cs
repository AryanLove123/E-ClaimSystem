using EClaim.Application.DTOs.Notifications;

public class NotificationSummaryDto
{
    public int UnreadCount { get; set; }
    public List<NotificationDto> Items { get; set; } = new();
}