using Asp.Versioning;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Billing.Queries.GetInvoiceById;
using HospitalApp.Core.Application.Features.Consults.Queries.GetConsultById;
using HospitalApp.Infrastructure.Shared.Settings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class PdfController(IMediator mediator, IPdfService pdf, IOptions<BusinessInfo> businessInfo) : BaseController
{
    private readonly BusinessInfo _business = businessInfo.Value;

    /// <summary>Download invoice PDF by invoice id.</summary>
    [HttpGet("invoice/{invoiceId:guid}")]
    public async Task<IActionResult> Invoice(Guid invoiceId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetInvoiceByIdQuery(invoiceId), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        var bytes = pdf.GenerateInvoice(result.Data!, _business.Name, _business.Address ?? string.Empty);
        return File(bytes, "application/pdf", $"factura-{result.Data!.InvoiceNumber}.pdf");
    }

    /// <summary>Download sick note PDF for a finished consult.</summary>
    [HttpGet("sick-note/{consultId:guid}")]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> SickNote(Guid consultId, [FromQuery] int days, CancellationToken ct)
    {
        if (days <= 0) return BadRequest(new { error = "Days must be positive." });

        var result = await mediator.Send(new GetConsultByIdQuery(consultId), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        var consult = result.Data!;
        var bytes = pdf.GenerateSickNote(consult, consult.PatientName, consult.DoctorName, days);
        return File(bytes, "application/pdf", $"reposo-{consultId}.pdf");
    }

    /// <summary>Download prescription PDF for a finished consult.</summary>
    [HttpGet("prescription/{consultId:guid}")]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> Prescription(Guid consultId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetConsultByIdQuery(consultId), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });

        var consult = result.Data!;
        var bytes = pdf.GeneratePrescription(consult, consult.PatientName, consult.DoctorName);
        return File(bytes, "application/pdf", $"receta-{consultId}.pdf");
    }
}
