using Asp.Versioning;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Prescriptions.Commands.AddPrescription;
using HospitalApp.Core.Application.Features.Prescriptions.Queries.GetPrescriptionsByConsult;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class PrescriptionsController(IMediator mediator, IDrugInteractionService drugInteractionService) : BaseController
{
    /// <summary>Get all prescriptions for a consult.</summary>
    [HttpGet("consult/{consultId:guid}")]
    public async Task<IActionResult> GetByConsult(Guid consultId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPrescriptionsByConsultQuery(consultId), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Add a prescription to an active consult.</summary>
    [HttpPost]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> Add([FromBody] AddPrescriptionWithConsultRequest request, CancellationToken ct)
    {
        var doctorId = GetCurrentUserId();
        var result = await mediator.Send(
            new AddPrescriptionCommand(request.ConsultId, request.Prescription, doctorId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetByConsult), new { consultId = request.ConsultId }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Check drug-drug interactions for a list of RxCUI codes (min 2).</summary>
    [HttpGet("check-interactions")]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> CheckInteractions(
        [FromQuery] List<string> rxcui,
        CancellationToken ct)
    {
        if (rxcui.Count < 2)
            return BadRequest(new { error = "At least 2 RxCUI codes required." });

        var alerts = await drugInteractionService.CheckInteractionsAsync(rxcui, ct);
        return Ok(new { alerts });
    }
}

public record AddPrescriptionWithConsultRequest(Guid ConsultId, AddPrescriptionRequest Prescription);
