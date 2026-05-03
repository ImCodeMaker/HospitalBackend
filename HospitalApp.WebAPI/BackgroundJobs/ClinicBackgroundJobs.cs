using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using HospitalApp.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.WebAPI.BackgroundJobs;

public class ClinicBackgroundJobs(
    IUnitOfWork uow,
    ApplicationDbContext db,
    IDashboardNotifier notifier,
    IEmailService email,
    ILogger<ClinicBackgroundJobs> logger)
{
    /// <summary>Daily: flag appointments as NoShow if past + still Confirmed/Scheduled.</summary>
    public async Task MarkNoShowAppointmentsAsync()
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-30);
        var stale = await uow.Appointments.FindAsync(
            a => a.ScheduledDate < cutoff &&
                 (a.Status == AppointmentStatusEnum.Scheduled || a.Status == AppointmentStatusEnum.Confirmed),
            CancellationToken.None);

        foreach (var apt in stale)
        {
            apt.Status = AppointmentStatusEnum.NoShow;
            apt.UpdatedAt = DateTime.UtcNow;
            uow.Appointments.Update(apt);
        }

        if (stale.Any())
        {
            await uow.SaveChangesAsync(CancellationToken.None);
            await notifier.NotifyAppointmentChangedAsync();
            logger.LogInformation("Marked {Count} appointments as NoShow", stale.Count());

            foreach (var apt in stale)
            {
                var patient = await uow.Patients.GetByIdAsync(apt.PatientId, CancellationToken.None);
                if (patient is null || string.IsNullOrEmpty(patient.Email))
                    continue;

                try
                {
                    var htmlBody = $"""
                        <p>Estimado/a <strong>{patient.FirstName} {patient.LastName}</strong>,</p>
                        <p>Notamos que no pudo asistir a su cita programada para el <strong>{apt.ScheduledDate:dd/MM/yyyy HH:mm}</strong>.</p>
                        <p>Le invitamos a reprogramar su cita a la brevedad posible para continuar con su atención médica.</p>
                        <p>Para reprogramar, por favor contáctenos o visite nuestras instalaciones.</p>
                        <p>Gracias,<br/>Lova Salud</p>
                        """;

                    await email.SendAsync(patient.Email, "Cita perdida — Lova Salud", htmlBody, CancellationToken.None);
                }
                catch
                {
                    // notification failure must not block operation
                }
            }
        }
    }

    /// <summary>Daily: send low-stock alert notifications (SignalR push to Admin/Doctor/Nurse).</summary>
    public async Task CheckLowStockAsync()
    {
        var lowStock = await uow.Medications.FindAsync(
            m => m.CurrentStock <= m.MinimumStockThreshold,
            CancellationToken.None);

        if (lowStock.Any())
        {
            await notifier.NotifyInventoryChangedAsync();
            logger.LogWarning("Low stock alert: {Count} medications below threshold. Items: {Items}",
                lowStock.Count(),
                string.Join(", ", lowStock.Select(m => m.GenericName)));
        }
    }

    /// <summary>Daily: auto-void invoices AwaitingPayment older than 30 days.</summary>
    public async Task VoidStaleInvoicesAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var stale = await uow.Invoices.FindAsync(
            i => i.Status == InvoiceStatusEnum.AwaitingPayment && i.CreatedAt < cutoff,
            CancellationToken.None);

        foreach (var inv in stale)
        {
            inv.Status = InvoiceStatusEnum.Cancelled;
            inv.UpdatedAt = DateTime.UtcNow;
            uow.Invoices.Update(inv);
        }

        if (stale.Any())
        {
            await uow.SaveChangesAsync(CancellationToken.None);
            await notifier.NotifyBillingChangedAsync();
            logger.LogInformation("Voided {Count} stale invoices", stale.Count());
        }
    }

    /// <summary>Weekly: purge audit logs older than 2 years.</summary>
    public async Task PurgeOldAuditLogsAsync()
    {
        var cutoff = DateTime.UtcNow.AddYears(-2);
        var deleted = await db.AuditLogs
            .Where(a => a.ChangedAt < cutoff)
            .ExecuteDeleteAsync();

        if (deleted > 0)
            logger.LogInformation("Purged {Count} audit log entries older than 2 years", deleted);
    }
}
