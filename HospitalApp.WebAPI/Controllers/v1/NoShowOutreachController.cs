using Asp.Versioning;
using HospitalApp.Core.Domain.Interfaces;
using HospitalApp.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class NoShowOutreachController(IUnitOfWork uow, ApplicationDbContext db) : BaseController
{
    /// <summary>Recent no-show outreach attempts grouped by patient.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int days = 90,
        [FromQuery] Guid? patientId = null,
        CancellationToken ct = default)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        var query = db.NoShowOutreachLogs
            .Where(l => l.ContactedAt >= since);
        if (patientId.HasValue) query = query.Where(l => l.PatientId == patientId.Value);

        var rows = await query
            .OrderByDescending(l => l.ContactedAt)
            .Select(l => new
            {
                l.Id,
                l.PatientId,
                PatientName = db.Patients.Where(p => p.Id == l.PatientId)
                    .Select(p => p.FirstName + " " + p.LastName).FirstOrDefault() ?? "—",
                l.AppointmentId,
                AppointmentDate = db.Appointments.Where(a => a.Id == l.AppointmentId)
                    .Select(a => (DateTime?)a.ScheduledDate).FirstOrDefault(),
                l.ContactedAt,
                l.Channel,
                l.Notes,
                l.PatientResponded,
            })
            .ToListAsync(ct);

        var settings = (await uow.ClinicSettings.FindAsync(_ => true, ct)).FirstOrDefault();
        var threshold = settings?.NoShowRepeatOffenderThreshold ?? 3;

        var groups = rows
            .GroupBy(r => r.PatientId)
            .Select(g => new
            {
                PatientId = g.Key,
                PatientName = g.First().PatientName,
                OutreachCount = g.Count(),
                LastContactedAt = g.Max(x => x.ContactedAt),
                IsRepeatOffender = g.Count() >= threshold,
                Entries = g.Select(x => new { x.Id, x.AppointmentId, x.AppointmentDate, x.ContactedAt, x.Channel, x.Notes, x.PatientResponded }).ToList(),
            })
            .OrderByDescending(g => g.OutreachCount)
            .ToList();

        return Ok(new { total = groups.Count, items = groups, threshold });
    }
}
