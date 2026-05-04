using Asp.Versioning;
using HospitalApp.Core.Application.Features.Consults.Commands.AttachConsultImage;
using HospitalApp.Core.Application.Features.Consults.Commands.CreateConsult;
using HospitalApp.Core.Application.Features.Consults.Commands.FinalizeConsult;
using HospitalApp.Core.Application.Features.Consults.Commands.UpdateConsult;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using HospitalApp.Core.Application.Features.Consults.Queries.GetConsultById;
using HospitalApp.Core.Application.Features.Consults.Queries.GetConsultImages;
using HospitalApp.Core.Application.Features.Consults.Queries.GetConsults;
using HospitalApp.Core.Application.Features.Consults.Queries.GetPatientConsults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class ConsultsController(IMediator mediator) : BaseController
{
    /// <summary>List all consults with optional status filter.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetConsultsQuery(status, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get all consults for a patient.</summary>
    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetByPatient(Guid patientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetPatientConsultsQuery(patientId, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get a consult by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetConsultByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Open a new consult for a patient.</summary>
    [HttpPost]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateConsultRequest request, CancellationToken ct)
    {
        var doctorId = GetCurrentUserId();
        var result = await mediator.Send(new CreateConsultCommand(request, doctorId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Data },
                new { id = result.Data, warning = result.Warning })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Update clinical details of an in-progress consult.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConsultRequest request, CancellationToken ct)
    {
        var doctorId = GetCurrentUserId();
        var result = await mediator.Send(new UpdateConsultCommand(id, request, doctorId), ct);
        return result.IsSuccess ? Ok(new { id = result.Data }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Finalize consult — required before billing. Deducts prescribed medications from inventory.</summary>
    [HttpPost("{id:guid}/finalize")]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> Finalize(Guid id, CancellationToken ct)
    {
        var doctorId = GetCurrentUserId();
        var result = await mediator.Send(new FinalizeConsultCommand(id, doctorId), ct);
        return result.IsSuccess ? Ok(new { id = result.Data }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get all images/attachments for a consult.</summary>
    [HttpGet("{consultId:guid}/images")]
    public async Task<IActionResult> GetImages(Guid consultId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetConsultImagesQuery(consultId), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Attach an image/document to a consult.</summary>
    [HttpPost("{consultId:guid}/images")]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> AttachImage(Guid consultId, [FromBody] AttachImageBody body, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var request = new AttachConsultImageRequest(consultId, body.FilePath, body.Description);
        var result = await mediator.Send(new AttachConsultImageCommand(request, userId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetImages), new { consultId }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record AttachImageBody(string FilePath, string? Description);
