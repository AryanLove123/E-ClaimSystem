using EClaim.Application.DTOs.Reports;
using EClaim.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace EClaim.Application.Services;

public class ReportService : IReportService
{
    private IEClaimDbContext _db;

    public ReportService(IEClaimDbContext db)
    {
        _db = db;
    }
    public async Task<ClaimsReportDto> GetClaimsReportAsync(CancellationToken ct = default)
    {
        var claims = await _db.Claims.ToListAsync(ct);

        return new ClaimsReportDto
        {
            TotalClaims = claims.Count,
            TotalRequestedAmount = claims.Sum(c => c.RequestedAmount),
            TotalApprovedAmount = claims.Sum(c => c.ApprovedAmount ?? 0),
            ByStatus = claims.GroupBy(c => c.Status.ToString())
                .Select(g => new StatusCountDto { Status = g.Key, Count = g.Count() }).ToList(),
            ByType = claims.GroupBy(c => c.ClaimType.ToString())
                .Select(g => new TypeCountDto { ClaimType = g.Key, Count = g.Count() }).ToList(),
            BySeverity = claims.GroupBy(c => c.Severity.ToString())
                .Select(g => new SeverityCountDto { Severity = g.Key, Count = g.Count() }).ToList(),
            ByMonth = claims.GroupBy(c => c.CreatedAt.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .Select(g => new MonthlyCountDto { Month = g.Key, Count = g.Count() }).ToList()
        };
    }

    public async Task<byte[]> ExportClaimsToExcelAsync(CancellationToken ct = default)
    {
        var claims = await _db.Claims.Include(c => c.Claimant).OrderByDescending(c => c.CreatedAt).ToListAsync(ct);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var sheet = workbook.Worksheets.Add("Claims");

        string[] headers = { "Claim Number", "Claimant", "Type", "Status", "Severity", "Requested", "Approved", "Created" };
        for (int i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];
        sheet.Row(1).Style.Font.Bold = true;

        var row = 2;
        foreach (var c in claims)
        {
            sheet.Cell(row, 1).Value = c.ClaimNumber;
            sheet.Cell(row, 2).Value = c.Claimant.FullName;
            sheet.Cell(row, 3).Value = c.ClaimType.ToString();
            sheet.Cell(row, 4).Value = c.Status.ToString();
            sheet.Cell(row, 5).Value = c.Severity.ToString();
            sheet.Cell(row, 6).Value = c.RequestedAmount;
            sheet.Cell(row, 7).Value = c.ApprovedAmount ?? 0;
            sheet.Cell(row, 8).Value = c.CreatedAt.ToString("yyyy-MM-dd");
            row++;
        }
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportClaimsToPdfAsync(CancellationToken ct = default)
    {
        var claims = await _db.Claims.Include(c => c.Claimant).OrderByDescending(c => c.CreatedAt).ToListAsync(ct);

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("E-Claim System — Claims Report").FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                    });

                    foreach (var header in new[] { "Claim #", "Claimant", "Type", "Status", "Requested" })
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(header).Bold();

                    foreach (var c in claims)
                    {
                        table.Cell().Padding(4).Text(c.ClaimNumber);
                        table.Cell().Padding(4).Text(c.Claimant.FullName);
                        table.Cell().Padding(4).Text(c.ClaimType.ToString());
                        table.Cell().Padding(4).Text(c.Status.ToString());
                        table.Cell().Padding(4).Text(c.RequestedAmount.ToString("C"));
                    }
                });
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated ");
                    x.Span(DateTime.UtcNow.ToString("u"));
                });
            });
        });

        return document.GeneratePdf();
    }
}