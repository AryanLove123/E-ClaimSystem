using EClaim.Application.DTOs.Reports;

namespace EClaim.Application.Interfaces;

public interface IReportService
{
    Task<ClaimsReportDto> GetClaimsReportAsync(CancellationToken ct = default);
    Task<byte[]> ExportClaimsToExcelAsync(CancellationToken ct = default);
    Task<byte[]> ExportClaimsToPdfAsync(CancellationToken ct = default);
}