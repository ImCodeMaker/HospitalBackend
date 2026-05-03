using Asp.Versioning;
using HospitalApp.Core.Application.Features.Patients.Commands.AnonymizePatient;
using HospitalApp.Core.Application.Features.Patients.Commands.ChangePatientStatus;
using HospitalApp.Core.Application.Features.Patients.Commands.CreatePatient;
using HospitalApp.Core.Application.Features.Patients.Commands.UpdatePatient;
using HospitalApp.Core.Application.Features.Patients.DTOs;
using HospitalApp.Core.Application.Features.Patients.Queries.CheckDuplicate;
using HospitalApp.Core.Application.Features.Patients.Queries.ExportPatient;
using HospitalApp.Core.Application.Features.Patients.Queries.GetPatientById;
using HospitalApp.Core.Application.Features.Patients.Queries.GetPatientTimeline;
using HospitalApp.Core.Application.Features.Patients.Queries.GetPatients;
using HospitalApp.Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class PatientsController(IMediator mediator) : BaseController
{
    /// <summary>Get paginated list of patients.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] PatientsStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetPatientsQuery(search, status, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get a single patient by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPatientByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Check if a patient with the given document already exists.</summary>
    [HttpGet("check-duplicate")]
    public async Task<IActionResult> CheckDuplicate(
        [FromQuery] DocumentTypeEnum documentType,
        [FromQuery] string documentNumber,
        CancellationToken ct)
    {
        var result = await mediator.Send(new CheckDuplicatePatientQuery(documentType, documentNumber), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Create a new patient.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreatePatientCommand(request), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Update patient demographics and insurance.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdatePatientCommand(id, request), ct);
        return result.IsSuccess ? Ok(new { id = result.Data }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Change patient status (e.g. Active → Archived).</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusBody body, CancellationToken ct)
    {
        var result = await mediator.Send(new ChangePatientStatusCommand(id, body.Status, body.Reason), ct);
        return result.IsSuccess ? Ok(new { id = result.Data }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Chronological timeline of all clinical activity for a patient.</summary>
    [HttpGet("{id:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(
        Guid id,
        [FromQuery] string? category,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetPatientTimelineQuery(id, category, from, to), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Export full patient data as GDPR data package (Admin only).</summary>
    [HttpGet("{id:guid}/export")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Export(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new ExportPatientQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Anonymize a patient record — replaces PII with ANONYMIZED and archives (Admin only).</summary>
    [HttpPost("{id:guid}/anonymize")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Anonymize(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new AnonymizePatientCommand(id), ct);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record ChangeStatusBody(PatientsStatus Status, string? Reason);
