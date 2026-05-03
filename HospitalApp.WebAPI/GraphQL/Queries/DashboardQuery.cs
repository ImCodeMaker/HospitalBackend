using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using HospitalApp.WebAPI.GraphQL.Types;
using HotChocolate.Authorization;

namespace HospitalApp.WebAPI.GraphQL.Queries;

public class DashboardQuery
{
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<DoctorDashboard> GetDoctorDashboard(
        Guid doctorId,
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayAppointments = await uow.Appointments.FindAsync(
            a => a.AssignedDoctorId == doctorId
                 && a.ScheduledDate >= today
                 && a.ScheduledDate < tomorrow, ct);

        var pendingConsults = await uow.Consults.CountAsync(
            c => c.DoctorId == doctorId && c.Status == ConsultStatusEnum.InProgress, ct);

        var unreviewedLabs = await uow.LabOrders.CountAsync(
            l => l.OrderedByDoctorId == doctorId
                 && l.Status == "Complete"
                 && !l.ResultReviewedByDoctor, ct);

        var appointmentSummaries = todayAppointments
            .OrderBy(a => a.ScheduledDate)
            .Select(a => new AppointmentSummary(
                a.Id,
                a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : "—",
                a.ScheduledDate,
                a.DurationMinutes,
                a.Type.ToString(),
                a.Status.ToString(),
                a.Reason))
            .ToList();

        return new DoctorDashboard(
            todayAppointments.Count,
            pendingConsults,
            unreviewedLabs,
            appointmentSummaries);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<AdminDashboard> GetAdminDashboard(
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalActivePatients = await uow.Patients.CountAsync(
            p => p.Status == PatientsStatus.Active, ct);

        var todayConsults = await uow.Consults.CountAsync(
            c => c.CreatedAt >= today && c.CreatedAt < tomorrow, ct);

        var monthConsults = await uow.Consults.CountAsync(
            c => c.CreatedAt >= monthStart, ct);

        var todayPayments = await uow.Payments.FindAsync(
            p => p.PaymentDate >= today && p.PaymentDate < tomorrow, ct);

        var monthPayments = await uow.Payments.FindAsync(
            p => p.PaymentDate >= monthStart, ct);

        var pendingInvoices = await uow.Invoices.CountAsync(
            i => i.Status == InvoiceStatusEnum.AwaitingPayment, ct);

        var allMeds = await uow.Medications.FindAsync(m => !m.IsExpired, ct);
        var lowStock = allMeds.Where(m => m.IsLowStock || m.IsOutOfStock).ToList();

        var lowStockAlerts = lowStock.Select(m => new LowStockAlert(
            m.Id,
            m.BrandName ?? m.GenericName,
            m.GenericName,
            m.CurrentStock,
            m.MinimumStockThreshold,
            m.IsOutOfStock)).ToList();

        return new AdminDashboard(
            totalActivePatients,
            todayConsults,
            monthConsults,
            todayPayments.Sum(p => p.Amount),
            monthPayments.Sum(p => p.Amount),
            pendingInvoices,
            lowStock.Count,
            lowStockAlerts);
    }

    [Authorize(Policy = "ClinicalStaff")]
    public async Task<ReceptionistDashboard> GetReceptionistDashboard(
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayAppointments = await uow.Appointments.FindAsync(
            a => a.ScheduledDate >= today && a.ScheduledDate < tomorrow, ct);

        var pendingBilling = await uow.Invoices.CountAsync(
            i => i.Status == InvoiceStatusEnum.AwaitingPayment, ct);

        var confirmedCount = todayAppointments.Count(a => a.Status == AppointmentStatusEnum.Confirmed);
        var attendedCount = todayAppointments.Count(a => a.Status == AppointmentStatusEnum.Attended);

        var summaries = todayAppointments
            .OrderBy(a => a.ScheduledDate)
            .Select(a => new AppointmentSummary(
                a.Id,
                a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : "—",
                a.ScheduledDate,
                a.DurationMinutes,
                a.Type.ToString(),
                a.Status.ToString(),
                a.Reason))
            .ToList();

        return new ReceptionistDashboard(
            todayAppointments.Count,
            confirmedCount,
            attendedCount,
            pendingBilling,
            summaries);
    }

    [Authorize(Policy = "ClinicalStaff")]
    public async Task<LabTechDashboard> GetLabTechDashboard(
        [Service] IUnitOfWork uow,
        CancellationToken ct)
    {
        var pendingOrders = await uow.LabOrders.FindAsync(
            l => l.Status == "Pending" || l.Status == "InProgress", ct);

        var urgentCount = pendingOrders.Count(
            l => l.Priority == LabTestPriorityEnum.Urgent || l.Priority == LabTestPriorityEnum.Stat);
        var inProgressCount = pendingOrders.Count(l => l.Status == "InProgress");

        var summaries = pendingOrders
            .OrderByDescending(l => l.Priority)
            .ThenBy(l => l.CreatedAt)
            .Select(l => new LabOrderSummary(
                l.Id,
                l.TestName,
                l.Priority.ToString(),
                l.Patient != null ? $"{l.Patient.FirstName} {l.Patient.LastName}" : "—",
                l.CreatedAt,
                l.IsExternal))
            .ToList();

        return new LabTechDashboard(
            pendingOrders.Count,
            urgentCount,
            inProgressCount,
            summaries);
    }
}
