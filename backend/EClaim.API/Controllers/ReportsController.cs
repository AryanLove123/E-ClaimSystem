using EClaim.Application.Common;
using EClaim.Application.DTOs.Reports;
using EClaim.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EClaim.API.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController : BaseApiController
{
    private IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("claims")]
    public async Task<IActionResult> GetClaimsReport(CancellationToken ct)
    {
        var report = await _reportService.GetClaimsReportAsync(ct);
        return Ok(ApiResponse<ClaimsReportDto>.Ok(report));
    }

    [HttpGet("claims/export/excel")]
    public async Task<IActionResult> ExportExcel(CancellationToken ct)
    {
        var bytes = await _reportService.ExportClaimsToExcelAsync(ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "claims-report.xlsx");
    }

    [HttpGet("claims/export/pdf")]
    public async Task<IActionResult> ExportPdf(CancellationToken ct)
    {
        var bytes = await _reportService.ExportClaimsToPdfAsync(ct);
        return File(bytes, "application/pdf", "claims-report.pdf");
    }
}
