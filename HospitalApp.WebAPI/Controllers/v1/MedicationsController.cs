using Asp.Versioning;
using HospitalApp.Core.Application.Features.Medications.Commands.AdjustStock;
using HospitalApp.Core.Application.Features.Medications.Commands.CreateMedication;
using HospitalApp.Core.Application.Features.Medications.DTOs;
using HospitalApp.Core.Application.Features.Medications.Queries.GetMedications;
using HospitalApp.Core.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class MedicationsController(IMediator mediator) : BaseController
{
    /// <summary>Get paginated medication inventory.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool? lowStockOnly,
        [FromQuery] bool? outOfStockOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetMedicationsQuery(search, lowStockOnly, outOfStockOnly, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Add a new medication to inventory (Admin only).</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateMedicationRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new CreateMedicationCommand(request, userId), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { id = result.Data }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Adjust stock (purchase receipt, manual adjustment, write-off).</summary>
    [HttpPost("{id:guid}/stock")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdjustStock(Guid id, [FromBody] AdjustStockBody body, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new AdjustStockCommand(id, body.Quantity, body.Type, body.Reason, userId), ct);
        return result.IsSuccess ? Ok(new { newStock = result.Data }) : StatusCode(result.StatusCode, new { error = result.Error });
    }
}

public record AdjustStockBody(int Quantity, StockTransactionTypeEnum Type, string? Reason);
