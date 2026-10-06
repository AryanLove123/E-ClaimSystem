namespace EClaim.Application.DTOs.Workflow;

public class ClaimWorkflowStatusDto
{
    public string WorkflowName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ClaimWorkflowStepStatusDto> Steps { get; set; } = new();
}

public class ClaimWorkflowStepStatusDto
{
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResponsibleRole { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public DateTime? CompletedAt { get; set; }
}
