using EClaim.Domain.Common;
using EClaim.Domain.Enums;

namespace EClaim.Domain.Entities;

public class Workflow : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ClaimType? ClaimType { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public ClaimSeverity? Severity { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();

}
