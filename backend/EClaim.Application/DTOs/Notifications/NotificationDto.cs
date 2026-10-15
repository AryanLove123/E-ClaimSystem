namespace EClaim.Application.DTOs.Notifications;

public class NotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public int? ClaimId { get; set; }
    public DateTime CreatedAt { get; set; }
}