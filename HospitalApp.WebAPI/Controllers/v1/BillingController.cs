using Asp.Versioning;
using HospitalApp.Core.Application.Features.Billing.Commands.CreateInvoice;
using HospitalApp.Core.Application.Features.Billing.Commands.ProcessPayment;
using HospitalApp.Core.Application.Features.Billing.Commands.ResolveInsuranceClaim;
using HospitalApp.Core.Application.Features.Billing.Commands.SubmitInsuranceClaim;
using HospitalApp.Core.Application.Features.Billing.DTOs;
using HospitalApp.Core.Application.Features.Billing.Queries.GetInvoiceById;
using HospitalApp.Core.Application.Features.Billing.Queries.GetInvoices;
using HospitalApp.Core.Application.Features.Billing.Queries.GetPatientInvoices;
using HospitalApp.Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "BillingStaff")]
public class BillingController(IMediator mediator) : BaseController
{
    /// <summary>List invoices with optional filters.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? patientId,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var utcFrom = from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : (DateTime?)null;
        var utcTo = to.HasValue ? DateTime.SpecifyKind(to.Value, DateTimeKind.Utc) : (DateTime?)null;
        var result = await mediator.Send(new GetInvoicesQuery(patientId, status, utcFrom, utcTo, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get invoice by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetInvoiceByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get all invoices for a patient.</summary>
    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetByPatient(
        Guid patientId,
        [FromQuery] InvoiceStatusEnum? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetPatientInvoicesQuery(patientId, status, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Create invoice for a finalized consult.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new CreateInvoiceCommand(request, userId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Record a payment against an invoice.</summary>
    [HttpPost("{id:guid}/payments")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] ProcessPaymentRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new ProcessPaymentCommand(id, request, userId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Submit an insurance claim for an invoice.</summary>
    [HttpPost("invoice/{id:guid}/submit-claim")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> SubmitClaim(Guid id, [FromBody] SubmitClaimRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new SubmitInsuranceClaimCommand(id, request.ClaimReferenceNumber, userId), ct);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Resolve an insurance claim for an invoice.</summary>
    [HttpPost("invoice/{id:guid}/resolve-claim")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> ResolveClaim(Guid id, [FromBody] ResolveClaimRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new ResolveInsuranceClaimCommand(id, request.Approved, request.ApprovedAmount, request.Notes, userId), ct);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record SubmitClaimRequest(string ClaimReferenceNumber);
public record ResolveClaimRequest(bool Approved, decimal? ApprovedAmount, string? Notes);
