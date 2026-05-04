using Asp.Versioning;
using HospitalApp.Core.Application.Common;
using AutoMapper;
using HospitalApp.Core.Application.Features.Caja.Commands.AddCashTransaction;
using HospitalApp.Core.Application.Features.Caja.Commands.CloseShift;
using HospitalApp.Core.Application.Features.Caja.Commands.OpenShift;
using HospitalApp.Core.Application.Features.Caja.DTOs;
using HospitalApp.Core.Application.Features.Caja.Queries.GetCurrentShift;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class CajaController(IMediator mediator, IUnitOfWork uow, IPdfService pdf, IMapper mapper) : BaseController
{
    /// <summary>Get the currently open shift.</summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
    {
        var result = await mediator.Send(new GetCurrentShiftQuery(), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Open a new cash register shift.</summary>
    [HttpPost("open")]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new OpenShiftCommand(request.OpeningBalance, userId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCurrent), new { }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Close the current shift and record the closing balance.</summary>
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> CloseShift(Guid id, [FromBody] CloseShiftRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new CloseShiftCommand(id, request.ClosingBalance, request.Notes, userId), ct);
        return result.IsSuccess ? Ok(new { id = result.Data }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Record a cash transaction against an open shift.</summary>
    [HttpPost("{id:guid}/transactions")]
    public async Task<IActionResult> AddTransaction(Guid id, [FromBody] AddTransactionRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(
            new AddCashTransactionCommand(id, request.Type, request.Amount, request.Description, request.InvoiceId, userId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetCurrent), new { }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>List transactions for a shift.</summary>
    [HttpGet("{shiftId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid shiftId, CancellationToken ct)
    {
        var shift = await uow.CajaShifts.GetByIdAsync(shiftId, ct);
        if (shift is null) return NotFound(new { error = "Shift not found." });
        var transactions = await uow.CashTransactions.FindAsync(t => t.ShiftId == shiftId, ct);
        return Ok(mapper.Map<List<CashTransactionDto>>(transactions.OrderBy(t => t.CreatedAt).ToList()));
    }

    /// <summary>Download a PDF report for a closed shift.</summary>
    [HttpGet("{shiftId:guid}/report")]
    public async Task<IActionResult> GetShiftReport(Guid shiftId, CancellationToken ct)
    {
        var shift = await uow.CajaShifts.GetByIdAsync(shiftId, ct);
        if (shift is null)
            return NotFound(new { error = "Shift not found." });

        if (shift.ClosedAt is null)
            return BadRequest(new { error = "Shift is still open; close it before generating a report." });

        var transactions = await uow.CashTransactions.FindAsync(t => t.ShiftId == shiftId, ct);

        var lines = transactions
            .OrderBy(t => t.CreatedAt)
            .Select(t => new ShiftTransactionLine(
                t.Type.ToString(),
                t.Amount,
                t.Description,
                t.IsApproved,
                t.CreatedAt))
            .ToList();

        var reportData = new ShiftReportData(
            ShiftId:        shift.Id,
            OpenedAt:       shift.CreatedAt,
            ClosedAt:       shift.ClosedAt.Value,
            OpenedByUserId: shift.OpenedByUserId.ToString(),
            OpeningBalance: shift.OpeningBalance,
            ClosingBalance: shift.ClosingBalance ?? 0m,
            ExpectedBalance: shift.ExpectedBalance ?? 0m,
            Discrepancy:    shift.Discrepancy ?? 0m,
            Notes:          shift.Notes,
            Transactions:   lines);

        var bytes = pdf.GenerateShiftReport(reportData);
        return File(bytes, "application/pdf", $"turno-{shiftId}.pdf");
    }
}

public record OpenShiftRequest(decimal OpeningBalance);
public record CloseShiftRequest(decimal ClosingBalance, string? Notes);
public record AddTransactionRequest(CashTransactionTypeEnum Type, decimal Amount, string? Description, Guid? InvoiceId);
