using EClaim.Domain.Common;
using EClaim.Domain.Enums;

namespace EClaim.Domain.Entities;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public NotificationType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public bool SentSuccessfully { get; set; }

    public int? ClaimId { get; set; }
    public Claim? Claim { get; set; }
}
