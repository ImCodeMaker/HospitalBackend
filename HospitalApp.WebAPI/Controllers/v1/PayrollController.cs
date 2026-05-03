using Asp.Versioning;
using HospitalApp.Core.Application.Features.Payroll.Commands.ApprovePayroll;
using HospitalApp.Core.Application.Features.Payroll.Commands.CreatePayroll;
using HospitalApp.Core.Application.Features.Payroll.Commands.MarkPayrollPaid;
using HospitalApp.Core.Application.Features.Payroll.DTOs;
using HospitalApp.Core.Application.Features.Payroll.Queries.GetEmployeePayroll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class PayrollController(IMediator mediator) : BaseController
{
    /// <summary>Get paginated payroll records for an employee.</summary>
    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeePayroll(
        Guid employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetEmployeePayrollQuery(employeeId, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Create a new payroll record.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePayrollRecordRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreatePayrollRecordCommand(request, GetCurrentUserId()), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetEmployeePayroll), new { employeeId = request.EmployeeId }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Approve a payroll record.</summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new ApprovePayrollCommand(id, GetCurrentUserId()), ct);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Mark an approved payroll record as paid.</summary>
    [HttpPost("{id:guid}/mark-paid")]
    public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkPaidRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new MarkPayrollPaidCommand(id, GetCurrentUserId(), request.PaymentDate, request.PaymentReference), ct);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record MarkPaidRequest(DateTime PaymentDate, string? PaymentReference);
