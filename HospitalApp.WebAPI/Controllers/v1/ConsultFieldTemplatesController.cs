using Asp.Versioning;
using HospitalApp.Core.Application.Features.ConsultFieldTemplates.Commands.CreateTemplate;
using HospitalApp.Core.Application.Features.ConsultFieldTemplates.Commands.DeleteTemplate;
using HospitalApp.Core.Application.Features.ConsultFieldTemplates.Commands.UpdateTemplate;
using HospitalApp.Core.Application.Features.ConsultFieldTemplates.DTOs;
using HospitalApp.Core.Application.Features.ConsultFieldTemplates.Queries.GetTemplatesBySpecialty;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ConsultFieldTemplatesController(IMediator mediator) : BaseController
{
    /// <summary>Get all consult field templates for a specialty.</summary>
    [HttpGet("by-specialty/{specialtyId:guid}")]
    [Authorize(Policy = "ClinicalStaff")]
    public async Task<IActionResult> GetBySpecialty(Guid specialtyId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTemplatesBySpecialtyQuery(specialtyId), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Create a new consult field template.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConsultFieldTemplateRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateConsultFieldTemplateCommand(request, GetCurrentUserId()), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetBySpecialty), new { specialtyId = request.SpecialtyId }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Update a consult field template.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConsultFieldTemplateRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateConsultFieldTemplateCommand(id, request), ct);
        return result.IsSuccess ? Ok(new { id = result.Data }) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Delete a consult field template.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteConsultFieldTemplateCommand(id), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
