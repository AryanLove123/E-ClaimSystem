namespace EClaim.Application.DTOs.Claims;

public class ClaimDto
{
    public int Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public int ClaimantId { get; set; }
    public string ClaimantName { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal? AdjustedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AssignedAdjusterName { get; set; }
    public string? AssignedApproverName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ClaimDocumentDto> Documents { get; set; } = new();
}
