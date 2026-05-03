using Asp.Versioning;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Reports.Queries.GetAccountsReceivable;
using HospitalApp.Core.Application.Features.Reports.Queries.GetDailyRevenue;
using HospitalApp.Core.Application.Features.Reports.Queries.GetInventoryReport;
using HospitalApp.WebAPI.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ReportsController(IMediator mediator, IPdfService pdf) : BaseController
{
    /// <summary>Daily revenue summary by payment method.</summary>
    [HttpGet("daily-revenue")]
    public async Task<IActionResult> DailyRevenue(
        [FromQuery] DateTime? date,
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetDailyRevenueQuery(date ?? DateTime.UtcNow.Date), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });

        return format.ToLower() switch
        {
            "csv" => File(
                CsvHelper.ToCsv(new[] { result.Data! }),
                "text/csv",
                $"revenue-{(date ?? DateTime.UtcNow.Date):yyyy-MM-dd}.csv"),
            "pdf" => File(
                pdf.GenerateDailyRevenueReport(result.Data!, date ?? DateTime.UtcNow.Date),
                "application/pdf",
                $"revenue-{(date ?? DateTime.UtcNow.Date):yyyy-MM-dd}.pdf"),
            _ => Ok(result.Data)
        };
    }

    /// <summary>Outstanding invoice balances aged by 0-30, 31-60, 61-90, 90+ days.</summary>
    [HttpGet("accounts-receivable")]
    public async Task<IActionResult> AccountsReceivable(
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var asOf = DateTime.UtcNow.Date;
        var result = await mediator.Send(new GetAccountsReceivableQuery(), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });

        return format.ToLower() switch
        {
            "csv" => File(
                CsvHelper.ToCsv(result.Data!),
                "text/csv",
                $"accounts-receivable-{asOf:yyyy-MM-dd}.csv"),
            "pdf" => File(
                pdf.GenerateAccountsReceivableReport(result.Data!, asOf),
                "application/pdf",
                $"accounts-receivable-{asOf:yyyy-MM-dd}.pdf"),
            _ => Ok(result.Data)
        };
    }

    /// <summary>Current inventory levels. Use lowStockOnly=true for alert report.</summary>
    [HttpGet("inventory")]
    [Authorize(Policy = "ClinicalStaff")]
    public async Task<IActionResult> Inventory(
        [FromQuery] bool lowStockOnly = false,
        [FromQuery] string format = "json",
        CancellationToken ct = default)
    {
        var asOf = DateTime.UtcNow.Date;
        var result = await mediator.Send(new GetInventoryReportQuery(lowStockOnly), ct);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, new { error = result.Error });

        return format.ToLower() switch
        {
            "csv" => File(
                CsvHelper.ToCsv(result.Data!),
                "text/csv",
                $"inventory-{asOf:yyyy-MM-dd}.csv"),
            "pdf" => File(
                pdf.GenerateInventoryReport(result.Data!, asOf),
                "application/pdf",
                $"inventory-{asOf:yyyy-MM-dd}.pdf"),
            _ => Ok(result.Data)
        };
    }
}
