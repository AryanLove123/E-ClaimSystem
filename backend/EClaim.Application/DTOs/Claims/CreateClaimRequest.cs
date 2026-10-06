namespace EClaim.Application.DTOs.Claims;

public class CreateClaimRequest
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
}
