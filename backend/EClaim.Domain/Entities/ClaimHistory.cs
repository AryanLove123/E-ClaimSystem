using EClaim.Domain.Common;
using EClaim.Domain.Enums;

namespace EClaim.Domain.Entities;

public class ClaimHistory: BaseEntity
{
    public int ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public ClaimStatus? OldStatus { get; set; }
    public ClaimStatus? NewStatus { get; set; }
    public int? PerformedByUserId { get; set; }
    public User? PerformedBy { get; set; }
    public string? Comments { get; set; }
}
