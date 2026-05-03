using Asp.Versioning;
using HospitalApp.Core.Application.Features.Recruitment.Commands.AdvanceStage;
using HospitalApp.Core.Application.Features.Recruitment.Commands.CreateApplication;
using HospitalApp.Core.Application.Features.Recruitment.DTOs;
using HospitalApp.Core.Application.Features.Recruitment.Queries.GetRecruitmentApplications;
using HospitalApp.Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class RecruitmentController(IMediator mediator) : BaseController
{
    /// <summary>Get paginated recruitment applications, optionally filtered by stage.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] RecruitmentStageEnum? stage,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetRecruitmentApplicationsQuery(stage, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Create a new recruitment application.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecruitmentApplicationRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateRecruitmentApplicationCommand(request), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Advance the stage of a recruitment application.</summary>
    [HttpPatch("{id:guid}/stage")]
    public async Task<IActionResult> AdvanceStage(Guid id, [FromBody] AdvanceRecruitmentStageRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new AdvanceRecruitmentStageCommand(id, request), ct);
        return result.IsSuccess ? Ok() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
