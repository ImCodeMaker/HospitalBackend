using Asp.Versioning;
using HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalAppointments;
using HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalConsults;
using HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalInvoices;
using HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalProfile;
using HospitalApp.Infrastructure.Identity.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/portal")]
[Authorize(Policy = "PatientPortal")]
public class PatientPortalController(
    IMediator mediator,
    UserManager<ApplicationUser> userManager) : BaseController
{
    private async Task<Guid?> GetLinkedPatientIdAsync()
    {
        var user = await userManager.FindByIdAsync(GetCurrentUserId().ToString());
        return user?.PatientId;
    }

    /// <summary>Get the logged-in patient's own profile.</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var patientId = await GetLinkedPatientIdAsync();
        if (patientId is null)
            return NotFound(new { error = "No patient record linked to this account." });

        var result = await mediator.Send(new GetPortalProfileQuery(patientId.Value), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get the logged-in patient's appointments.</summary>
    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] bool upcomingOnly = false,
        CancellationToken ct = default)
    {
        var patientId = await GetLinkedPatientIdAsync();
        if (patientId is null)
            return NotFound(new { error = "No patient record linked to this account." });

        var result = await mediator.Send(new GetPortalAppointmentsQuery(patientId.Value, upcomingOnly), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get the logged-in patient's consult history (summaries, no raw clinical notes).</summary>
    [HttpGet("consults")]
    public async Task<IActionResult> GetConsults(CancellationToken ct)
    {
        var patientId = await GetLinkedPatientIdAsync();
        if (patientId is null)
            return NotFound(new { error = "No patient record linked to this account." });

        var result = await mediator.Send(new GetPortalConsultsQuery(patientId.Value), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get the logged-in patient's invoices.</summary>
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices(CancellationToken ct)
    {
        var patientId = await GetLinkedPatientIdAsync();
        if (patientId is null)
            return NotFound(new { error = "No patient record linked to this account." });

        var result = await mediator.Send(new GetPortalInvoicesQuery(patientId.Value), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
