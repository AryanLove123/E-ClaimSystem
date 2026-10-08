namespace EClaim.Application.DTOs.Claims;

public class AdjustClaimRequest
{
    public decimal AdjustedAmount { get; set; }
    public string? Comments { get; set; }
}
