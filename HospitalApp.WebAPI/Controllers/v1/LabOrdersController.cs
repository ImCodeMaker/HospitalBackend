using Asp.Versioning;
using HospitalApp.Core.Application.Features.LabOrders.Commands.AddLabResult;
using HospitalApp.Core.Application.Features.LabOrders.Commands.CreateLabOrder;
using HospitalApp.Core.Application.Features.LabOrders.Commands.MarkResultReviewed;
using HospitalApp.Core.Application.Features.LabOrders.Commands.UpdateLabOrderStatus;
using HospitalApp.Core.Application.Features.LabOrders.Queries.GetLabOrders;
using HospitalApp.Core.Application.Features.LabOrders.Queries.GetLabOrdersByConsult;
using HospitalApp.Core.Application.Features.LabOrders.Queries.GetLabResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class LabOrdersController(IMediator mediator) : BaseController
{
    /// <summary>List all lab orders with optional status/priority filter.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetLabOrdersQuery(status, priority), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get results for a lab order.</summary>
    [HttpGet("{id:guid}/results")]
    public async Task<IActionResult> GetResults(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLabResultsQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Update status of a lab order.</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateLabOrderStatusBody body, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateLabOrderStatusCommand(id, body.Status), ct);
        return result.IsSuccess ? Ok(new { id = result.Data }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get all lab orders for a consult.</summary>
    [HttpGet("consult/{consultId:guid}")]
    public async Task<IActionResult> GetByConsult(Guid consultId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLabOrdersByConsultQuery(consultId), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Order a lab test from an active consult.</summary>
    [HttpPost]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateLabOrderRequest request, CancellationToken ct)
    {
        var doctorId = GetCurrentUserId();
        var result = await mediator.Send(new CreateLabOrderCommand(request, doctorId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetByConsult), new { consultId = request.ConsultId }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Add result(s) to a lab order.</summary>
    [HttpPost("{id:guid}/results")]
    public async Task<IActionResult> AddResult(Guid id, [FromBody] AddLabResultRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new AddLabResultCommand(id, request, userId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetByConsult), new { }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Mark lab results as reviewed by the ordering doctor.</summary>
    [HttpPost("{id:guid}/reviewed")]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> MarkReviewed(Guid id, CancellationToken ct)
    {
        var doctorId = GetCurrentUserId();
        var result = await mediator.Send(new MarkResultReviewedCommand(id, doctorId), ct);
        return result.IsSuccess ? Ok(new { id = result.Data }) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record UpdateLabOrderStatusBody(string Status);
