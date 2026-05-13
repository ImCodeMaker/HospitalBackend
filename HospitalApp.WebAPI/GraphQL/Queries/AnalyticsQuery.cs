using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using HospitalApp.Infrastructure.Persistence.Context;
using HospitalApp.WebAPI.GraphQL.Types;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.WebAPI.GraphQL.Queries;

[ExtendObjectType<DashboardQuery>]
public class AnalyticsQuery
{
    [Authorize(Policy = "AdminOnly")]
    public async Task<List<RevenueTrendPoint>> GetRevenueTrend(
        int days,
        [Service] ApplicationDbContext db,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var rows = await db.Payments
            .Where(p => p.PaymentDate >= since)
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new RevenueTrendPoint(g.Key, g.Sum(p => p.Amount)))
            .OrderBy(p => p.Date)
            .ToListAsync(ct);

        // Fill missing days with zero so the chart is continuous
        var byDate = rows.ToDictionary(r => r.Date);
        var filled = new List<RevenueTrendPoint>();
        for (var d = since; d <= DateTime.UtcNow.Date; d = d.AddDays(1))
            filled.Add(byDate.TryGetValue(d, out var p) ? p : new RevenueTrendPoint(d, 0m));
        return filled;
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<SpecialtyVolumeSlice>> GetConsultsBySpecialty(
        int days,
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var consults = await uow.Consults.FindAsync(c => c.CreatedAt >= since, ct);
        return consults
            .Where(c => c.Specialty != null)
            .GroupBy(c => new { c.SpecialtyId, Name = c.Specialty!.Name })
            .Select(g => new SpecialtyVolumeSlice(g.Key.SpecialtyId, g.Key.Name, g.Count()))
            .OrderByDescending(s => s.Count)
            .ToList();
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<DiagnosisFrequency>> GetTopDiagnoses(
        int top,
        int days,
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var consults = await uow.Consults.FindAsync(
            c => c.CreatedAt >= since && c.DiagnosisCodes != null, ct);

        var bag = consults
            .SelectMany(c => (c.DiagnosisCodes ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(code => new { Code = code, c.DiagnosisDescription }));

        return bag
            .GroupBy(x => x.Code)
            .Select(g => new DiagnosisFrequency(g.Key, g.First().DiagnosisDescription ?? "—", g.Count()))
            .OrderByDescending(d => d.Count)
            .Take(top)
            .ToList();
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<HourlyVolumeCell>> GetVolumeHeatmap(
        int days,
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var appts = await uow.Appointments.FindAsync(a => a.ScheduledDate >= since, ct);
        return appts
            .GroupBy(a => new { Dow = (int)a.ScheduledDate.DayOfWeek, Hour = a.ScheduledDate.Hour })
            .Select(g => new HourlyVolumeCell(g.Key.Dow, g.Key.Hour, g.Count()))
            .ToList();
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<DoctorRevenueSlice>> GetRevenueByDoctor(
        int days,
        [Service] ApplicationDbContext db,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);

        var rows = await db.Invoices
            .Where(i => i.CreatedAt >= since && i.Consult != null)
            .Select(i => new { i.Consult!.DoctorId, i.PaidAmount })
            .ToListAsync(ct);

        var grouped = rows.GroupBy(r => r.DoctorId).ToList();
        var result = new List<DoctorRevenueSlice>();
        foreach (var g in grouped)
        {
            var name = await db.Users.Where(u => u.Id == g.Key)
                .Select(u => u.FirstName + " " + u.LastName).FirstOrDefaultAsync(ct) ?? "—";
            result.Add(new DoctorRevenueSlice(g.Key, name, g.Sum(x => x.PaidAmount), g.Count()));
        }
        return result.OrderByDescending(r => r.TotalRevenue).ToList();
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<PaymentMethodSlice>> GetPaymentMethodDistribution(
        int days,
        [Service] ApplicationDbContext db,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);

        var rows = await db.Payments
            .Where(p => p.PaymentDate >= since)
            .Select(p => new { p.Method, p.Amount })
            .ToListAsync(ct);

        return rows
            .GroupBy(p => p.Method)
            .Select(g => new PaymentMethodSlice(g.Key.ToString(), g.Count(), g.Sum(x => x.Amount)))
            .OrderByDescending(s => s.Total)
            .ToList();
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<double> GetAverageLabTurnaroundHours(
        int days,
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var orders = await uow.LabOrders.FindAsync(
            o => o.CreatedAt >= since && o.ResultsAvailableAt != null, ct);
        if (!orders.Any()) return 0;
        return orders.Average(o => (o.ResultsAvailableAt!.Value - o.CreatedAt).TotalHours);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<double> GetNoShowRate(
        int days,
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var appts = await uow.Appointments.FindAsync(a => a.ScheduledDate >= since && a.ScheduledDate < DateTime.UtcNow, ct);
        var total = appts.Count();
        if (total == 0) return 0;
        var noShow = appts.Count(a => a.Status == AppointmentStatusEnum.NoShow);
        return Math.Round((double)noShow / total * 100, 2);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<DemographicsReport> GetPatientDemographics(
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var patients = await uow.Patients.FindAsync(p => p.Status == PatientsStatus.Active, ct);
        var list = patients.ToList();

        AgeBucket Bucket(string label, Func<int, bool> match) =>
            new(label, list.Count(p => match(DateTime.UtcNow.Year - p.BirthDate.Year)));

        var buckets = new List<AgeBucket>
        {
            Bucket("0-12", a => a >= 0 && a <= 12),
            Bucket("13-17", a => a >= 13 && a <= 17),
            Bucket("18-29", a => a >= 18 && a <= 29),
            Bucket("30-44", a => a >= 30 && a <= 44),
            Bucket("45-64", a => a >= 45 && a <= 64),
            Bucket("65+", a => a >= 65),
        };

        var gender = list
            .GroupBy(p => p.Gender.ToString())
            .Select(g => new GenderSlice(g.Key, g.Count()))
            .ToList();

        return new DemographicsReport(list.Count, buckets, gender);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<PrescriptionFrequency>> GetPrescriptionAnalysis(
        int top,
        int days,
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);
        var prescriptions = await uow.Prescriptions.FindAsync(
            p => p.CreatedAt >= since && p.Consult != null, ct);

        return prescriptions
            .GroupBy(p => p.DrugName)
            .Select(g => new PrescriptionFrequency(
                g.Key,
                g.Count(),
                g.Select(x => x.Consult!.PatientId).Distinct().Count()))
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToList();
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<ConsultVolumeByDoctorRow>> GetConsultVolumeByDoctor(
        int days,
        [Service] ApplicationDbContext db,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);

        var raw = await db.Consults
            .Where(c => c.CreatedAt >= since)
            .Select(c => new { c.DoctorId, c.StartedAt, c.FinishedAt })
            .ToListAsync(ct);

        var grouped = raw.GroupBy(r => r.DoctorId).ToList();
        var result = new List<ConsultVolumeByDoctorRow>();
        foreach (var g in grouped)
        {
            var name = await db.Users.Where(u => u.Id == g.Key)
                .Select(u => u.FirstName + " " + u.LastName).FirstOrDefaultAsync(ct) ?? "—";
            var durations = g.Where(c => c.StartedAt.HasValue && c.FinishedAt.HasValue)
                             .Select(c => (c.FinishedAt!.Value - c.StartedAt!.Value).TotalMinutes)
                             .ToList();
            var avg = durations.Count > 0 ? durations.Average() : 0;
            result.Add(new ConsultVolumeByDoctorRow(g.Key, name, g.Count(), avg));
        }
        return result.OrderByDescending(r => r.ConsultCount).ToList();
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<StaffPerformanceRow>> GetStaffPerformance(
        int days,
        [Service] ApplicationDbContext db,
        CancellationToken ct)
    {
        var since = DateTime.UtcNow.Date.AddDays(-days);

        var consultsByDoctor = await db.Consults
            .Where(c => c.CreatedAt >= since)
            .GroupBy(c => c.DoctorId)
            .Select(g => new
            {
                DoctorId = g.Key,
                PatientsAttended = g.Select(c => c.PatientId).Distinct().Count(),
                Completed = g.Count(c => c.Status == ConsultStatusEnum.Finished),
            })
            .ToListAsync(ct);

        var revenueByDoctor = await db.Invoices
            .Where(i => i.CreatedAt >= since && i.Consult != null)
            .GroupBy(i => i.Consult!.DoctorId)
            .Select(g => new { DoctorId = g.Key, Revenue = g.Sum(i => i.PaidAmount) })
            .ToListAsync(ct);

        var revLookup = revenueByDoctor.ToDictionary(x => x.DoctorId, x => x.Revenue);

        var rows = new List<StaffPerformanceRow>();
        foreach (var c in consultsByDoctor)
        {
            var name = await db.Users.Where(u => u.Id == c.DoctorId)
                .Select(u => u.FirstName + " " + u.LastName).FirstOrDefaultAsync(ct) ?? "—";
            rows.Add(new StaffPerformanceRow(
                c.DoctorId, name, "Doctor",
                c.PatientsAttended,
                revLookup.GetValueOrDefault(c.DoctorId, 0m),
                c.Completed));
        }
        return rows.OrderByDescending(r => r.RevenueGenerated).ToList();
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<List<ControlledSubstanceEntry>> GetControlledSubstanceLog(
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var meds = await uow.Medications.FindAsync(m => m.IsControlledSubstance, ct);
        var since30 = DateTime.UtcNow.AddDays(-30);
        var result = new List<ControlledSubstanceEntry>();

        foreach (var m in meds)
        {
            var dispensed = await uow.StockTransactions.FindAsync(
                t => t.MedicationId == m.Id && t.CreatedAt >= since30 && t.Quantity < 0, ct);
            var qty = dispensed.Sum(t => Math.Abs(t.Quantity));
            var last = dispensed.OrderByDescending(t => t.CreatedAt).FirstOrDefault();

            result.Add(new ControlledSubstanceEntry(
                m.Id,
                m.BrandName ?? m.GenericName,
                m.ControlledSubstanceClass,
                m.CurrentStock,
                qty,
                last?.CreatedAt));
        }
        return result.OrderBy(c => c.MedicationName).ToList();
    }
}
