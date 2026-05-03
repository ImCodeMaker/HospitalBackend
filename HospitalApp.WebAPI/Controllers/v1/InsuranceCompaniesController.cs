using Asp.Versioning;
using HospitalApp.Core.Application.Features.InsuranceCompanies.Commands.CreateInsuranceCompany;
using HospitalApp.Core.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class InsuranceCompaniesController(IMediator mediator) : BaseController
{
    /// <summary>List insurance companies.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetInsuranceCompaniesQuery(activeOnly), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Register an insurance company.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateInsuranceCompanyRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateInsuranceCompanyCommand(request), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
