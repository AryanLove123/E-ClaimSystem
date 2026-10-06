namespace EClaim.Application.DTOs.Claims;

public class ClaimFilterRequest
{
    public string? ClaimNumber { get; set; }
    public string? PolicyNumber { get; set; }
    public string? ClaimType { get; set; }
    public string? Status { get; set; }
    public string? Severity { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
