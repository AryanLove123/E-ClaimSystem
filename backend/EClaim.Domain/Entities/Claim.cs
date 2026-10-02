using EClaim.Domain.Common;
using EClaim.Domain.Enums;

namespace EClaim.Domain.Entities;

public class Claim: BaseEntity
{
    public string ClaimNumber { get; set; } = string.Empty;
    public int ClaimantId { get; set; }
    public User Claimant { get; set; } = null!;
    public string PolicyNumber { get; set; } = string.Empty;
    public ClaimType ClaimType { get; set; }
    public DateTime IncidentDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public decimal RequestedAmount { get; set; }
    public decimal? AdjustedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }

    public ClaimSeverity Severity { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;
    public byte[]? RowVersion { get; set; }

    public int? AssignedAdjusterId { get; set; }
    public User? AssignedAdjuster { get; set; }

    public int? AssignedApproverId { get; set; }
    public User? AssignedApprover { get; set; }
    public ICollection<ClaimDocument> Documents { get; set; } = new List<ClaimDocument>();
    public ICollection<ClaimHistory> History { get; set; } = new List<ClaimHistory>();
    public ClaimWorkflow? ClaimWorkflow { get; set; }
}
