namespace EClaim.Application.DTOs.Workflow;

public class WorkflowStepDto
{
    public int Id { get; set; }
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResponsibleRole { get; set; } = string.Empty;
    public bool AllowReject { get; set; }
}
