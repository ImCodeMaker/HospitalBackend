using Asp.Versioning;
using HospitalApp.Core.Application.Features.AuditLog.DTOs;
using HospitalApp.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AuditController(ApplicationDbContext db) : BaseController
{
    /// <summary>Query audit log with optional filters. Admin only.</summary>
    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? tableName,
        [FromQuery] Guid? recordId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var query = db.AuditLogs.AsQueryable();

        if (tableName is not null)
            query = query.Where(a => a.TableName == tableName);
        if (recordId.HasValue)
            query = query.Where(a => a.RecordId == recordId.Value);
        if (from.HasValue)
            query = query.Where(a => a.ChangedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.ChangedAt <= to.Value);

        var total = await query.CountAsync(ct);
        var logs = await query
            .OrderByDescending(a => a.ChangedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                TableName = a.TableName,
                RecordId = a.RecordId,
                Action = a.Action,
                ChangedBy = a.ChangedBy,
                ChangedAt = a.ChangedAt,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
            })
            .ToListAsync(ct);

        return Ok(new { total, page, pageSize, items = logs });
    }
}
