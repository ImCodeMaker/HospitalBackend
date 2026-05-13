using Asp.Versioning;
using HospitalApp.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ControlledSubstanceLogController(IUnitOfWork uow) : BaseController
{
    /// <summary>Immutable ledger of controlled substance movements. For DEA / regulatory inspection.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? medicationId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        var since = from ?? DateTime.UtcNow.AddDays(-90);
        var until = to ?? DateTime.UtcNow;
        var entries = await uow.ControlledSubstanceLogs.FindAsync(
            l => l.CreatedAt >= since && l.CreatedAt <= until &&
                 (medicationId == null || l.MedicationId == medicationId), ct);

        var total = entries.Count();
        var page1 = entries
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new { total, page, pageSize, items = page1 });
    }
}
