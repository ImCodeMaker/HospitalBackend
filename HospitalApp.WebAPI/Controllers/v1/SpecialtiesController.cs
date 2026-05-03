using Asp.Versioning;
using HospitalApp.Core.Application.Features.Specialties.Commands.CreateSpecialty;
using HospitalApp.Core.Application.Features.Specialties.Queries.GetSpecialties;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class SpecialtiesController(IMediator mediator) : BaseController
{
    /// <summary>List all specialties.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetSpecialtiesQuery(), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Create a specialty.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateSpecialtyRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateSpecialtyCommand(request), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
