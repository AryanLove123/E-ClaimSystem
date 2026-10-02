using EClaim.Domain.Common;
using EClaim.Domain.Enums;

namespace EClaim.Domain.Entities;

public class ClaimWorkflow : BaseEntity
{
    public int ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;
    public int WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
    public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.Active;
    public ICollection<ClaimWorkflowStep> Steps { get; set; } = new List<ClaimWorkflowStep>();
}
