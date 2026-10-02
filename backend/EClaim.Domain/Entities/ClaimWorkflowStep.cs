using EClaim.Domain.Common;
using EClaim.Domain.Enums;

namespace EClaim.Domain.Entities;

public class ClaimWorkflowStep: BaseEntity
{
    public int ClaimWorkflowId { get; set; }
    public ClaimWorkflow ClaimWorkflow { get; set; } = null!;
    public int WorkflowStepId { get; set; }
    public WorkflowStep WorkflowStep { get; set; } = null!;
    public int StepOrder { get; set; }
    public WorkflowStepStatus Status { get; set; } = WorkflowStepStatus.Pending;
    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Comments { get; set; }
}
