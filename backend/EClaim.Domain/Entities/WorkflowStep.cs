using EClaim.Domain.Common;
using EClaim.Domain.Enums;

namespace EClaim.Domain.Entities;

public class WorkflowStep: BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int WorkflowId { get; set; }
    public Workflow Workflow { get; set; } = null!;
    public int StepOrder { get; set; }
    public ClaimStepRole ResponsibleRole { get; set; }
    public bool AllowReject { get; set; } = true;

}
