using Asp.Versioning;
using HospitalApp.Core.Application.Features.HR.Commands.CreateEmployee;
using HospitalApp.Core.Application.Features.HR.Queries.GetEmployeePerformance;
using HospitalApp.Core.Application.Features.HR.Queries.GetEmployees;
using HospitalApp.Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class HRController(IMediator mediator) : BaseController
{
    /// <summary>List employees with optional role and status filters.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] EmployeeRoleEnum? role,
        [FromQuery] EmployeeStatusEnum? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetEmployeesQuery(role, status, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Create a new employee record.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateEmployeeCommand(request), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get rolling 30-day performance metrics for an employee.</summary>
    [HttpGet("{id:guid}/performance")]
    [Authorize] // doctors can see own performance
    public async Task<IActionResult> GetPerformance(
        Guid id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetEmployeePerformanceQuery(id, from, to), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
