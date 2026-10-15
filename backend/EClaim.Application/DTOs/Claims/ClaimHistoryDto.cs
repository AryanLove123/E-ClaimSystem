namespace EClaim.Application.DTOs.Claims;

public class ClaimHistoryDto
{
    public string Action { get; set; } = string.Empty;
    public string? OldStatus { get; set; }
    public string? NewStatus { get; set; }
    public string? PerformedByName { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
}
