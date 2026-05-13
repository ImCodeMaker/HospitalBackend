using Asp.Versioning;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class NcfSequencesController(IUnitOfWork uow) : BaseController
{
    /// <summary>List all NCF sequences (active and historical).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var seqs = await uow.NcfSequences.FindAsync(_ => true, ct);
        return Ok(seqs
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                Type = s.Type.ToString(),
                Prefix = s.Type.GetPrefix(),
                s.CurrentSequence,
                s.MaxSequence,
                Remaining = s.MaxSequence - s.CurrentSequence + 1,
                s.ExpirationDate,
                s.IsActive,
                IsExpired = DateTime.UtcNow > s.ExpirationDate,
                IsExhausted = s.CurrentSequence > s.MaxSequence,
            }));
    }

    /// <summary>Register a new authorized NCF range. Deactivates the current active range of the same type.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNcfRangeRequest body, CancellationToken ct)
    {
        if (body.MaxSequence < body.StartSequence)
            return BadRequest(new { error = "MaxSequence must be >= StartSequence." });
        if (body.ExpirationDate <= DateTime.UtcNow)
            return BadRequest(new { error = "ExpirationDate must be in the future." });

        var existingActive = await uow.NcfSequences
            .FindAsync(s => s.Type == body.Type && s.IsActive, ct);
        foreach (var s in existingActive)
        {
            s.IsActive = false;
            s.UpdatedAt = DateTime.UtcNow;
            uow.NcfSequences.Update(s);
        }

        var fresh = new NcfSequence
        {
            Type = body.Type,
            CurrentSequence = body.StartSequence,
            MaxSequence = body.MaxSequence,
            ExpirationDate = body.ExpirationDate,
            IsActive = true,
        };
        await uow.NcfSequences.AddAsync(fresh, ct);
        await uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetAll), new { id = fresh.Id });
    }
}

public record CreateNcfRangeRequest(NcfTypeEnum Type, long StartSequence, long MaxSequence, DateTime ExpirationDate);
