using Asp.Versioning;
using HospitalApp.Core.Application.Features.Appointments.Commands.CreateAppointment;
using HospitalApp.Core.Application.Features.Appointments.Commands.UpdateAppointmentStatus;
using HospitalApp.Core.Application.Features.Appointments.DTOs;
using HospitalApp.Core.Application.Features.Appointments.Queries.GetAppointments;
using HospitalApp.Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class AppointmentsController(IMediator mediator) : BaseController
{
    /// <summary>Get appointments with optional filters.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? doctorId,
        [FromQuery] Guid? patientId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] AppointmentStatusEnum? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var utcFrom = from.HasValue ? DateTime.SpecifyKind(from.Value, DateTimeKind.Utc) : (DateTime?)null;
        var utcTo = to.HasValue ? DateTime.SpecifyKind(to.Value, DateTimeKind.Utc) : (DateTime?)null;
        var result = await mediator.Send(new GetAppointmentsQuery(doctorId, patientId, utcFrom, utcTo, status, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Schedule a new appointment.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new CreateAppointmentCommand(request, userId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Update appointment status (confirm, cancel, no-show, complete).</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateAppointmentStatusCommand(id, request.NewStatus, request.Notes), ct);
        return result.IsSuccess ? Ok(new { id = result.Data }) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record UpdateAppointmentStatusRequest(AppointmentStatusEnum NewStatus, string? Notes);
