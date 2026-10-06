namespace EClaim.Application.DTOs.Workflow;

public class CreateWorkflowRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ClaimType { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Severity { get; set; }
    public int Priority { get; set; }
    public List<CreateWorkflowStepRequest> Steps { get; set; } = new();
}

public class CreateWorkflowStepRequest
{
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResponsibleRole { get; set; } = string.Empty;
    public bool AllowReject { get; set; } = true;
}

