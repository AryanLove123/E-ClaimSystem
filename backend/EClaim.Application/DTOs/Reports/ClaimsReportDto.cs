namespace EClaim.Application.DTOs.Reports;

public class ClaimsReportDto
{
    public int TotalClaims { get; set; }
    public decimal TotalRequestedAmount { get; set; }
    public decimal TotalApprovedAmount { get; set; }
    public List<StatusCountDto> ByStatus { get; set; } = new();
    public List<TypeCountDto> ByType { get; set; } = new();
    public List<SeverityCountDto> BySeverity { get; set; } = new();
    public List<MonthlyCountDto> ByMonth { get; set; } = new();
}

public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}
public class TypeCountDto
{
    public string ClaimType { get; set; } = string.Empty;
    public int Count { get; set; }
}
public class SeverityCountDto
{
    public string Severity { get; set; } = string.Empty;
    public int Count { get; set; }
}
public class MonthlyCountDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}
