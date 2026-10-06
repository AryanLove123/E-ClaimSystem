namespace EClaim.Application.DTOs.Workflow;

public class WorkflowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ClaimType { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Severity { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new();
}
