using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Reports.DTOs;
using HospitalApp.Core.Domain.Entities;
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
    ISmsService sms,
    ILogger<ClinicBackgroundJobs> logger)
{
    /// <summary>Every 15 min: send 24h and 2h reminders for upcoming appointments.</summary>
    public async Task SendAppointmentRemindersAsync()
    {
        var now = DateTime.UtcNow;
        var window24Start = now.AddHours(23);
        var window24End = now.AddHours(25);
        var window2Start = now.AddMinutes(90);
        var window2End = now.AddMinutes(150);

        await SendWindowAsync(window24Start, window24End, is24h: true);
        await SendWindowAsync(window2Start, window2End, is24h: false);
    }

    private async Task SendWindowAsync(DateTime windowStart, DateTime windowEnd, bool is24h)
    {
        var pending = await uow.Appointments.FindAsync(a =>
            a.ScheduledDate >= windowStart &&
            a.ScheduledDate < windowEnd &&
            (a.Status == AppointmentStatusEnum.Scheduled || a.Status == AppointmentStatusEnum.Confirmed) &&
            (is24h ? !a.Reminder24hSent : !a.Reminder2hSent),
            CancellationToken.None);

        if (!pending.Any()) return;

        foreach (var apt in pending)
        {
            var patient = await uow.Patients.GetByIdAsync(apt.PatientId, CancellationToken.None);
            if (patient is null) continue;

            var whenLabel = is24h ? "mañana" : "en aproximadamente 2 horas";
            var subject = is24h ? "Recordatorio: cita mañana" : "Recordatorio: cita próxima";

            var body = $"""
                <p>Estimado/a <strong>{patient.FirstName} {patient.LastName}</strong>,</p>
                <p>Le recordamos que tiene una cita {whenLabel}:</p>
                <ul>
                    <li><strong>Fecha y hora:</strong> {apt.ScheduledDate:dd/MM/yyyy HH:mm}</li>
                    <li><strong>Tipo:</strong> {apt.Type}</li>
                    <li><strong>Duración estimada:</strong> {apt.DurationMinutes} minutos</li>
                </ul>
                <p>Si no podrá asistir, contáctenos con anticipación.</p>
                <p>Gracias,<br/>Lova Salud</p>
                """;

            try
            {
                if (!string.IsNullOrEmpty(patient.Email))
                    await email.SendAsync(patient.Email, subject, body, CancellationToken.None);
                if (!string.IsNullOrEmpty(patient.Phone))
                    await sms.SendAsync(patient.Phone,
                        $"Recordatorio: cita {whenLabel} {apt.ScheduledDate:dd/MM HH:mm} en Lova Salud.",
                        CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send {Type} reminder for appointment {Id}", is24h ? "24h" : "2h", apt.Id);
            }

            if (is24h) apt.Reminder24hSent = true;
            else apt.Reminder2hSent = true;

            apt.UpdatedAt = DateTime.UtcNow;
            uow.Appointments.Update(apt);
        }

        await uow.SaveChangesAsync(CancellationToken.None);
        logger.LogInformation("Sent {Count} {Type} appointment reminders", pending.Count(), is24h ? "24h" : "2h");
    }

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
                if (patient is null) continue;

                var emailed = false;
                var smsSent = false;
                if (!string.IsNullOrEmpty(patient.Email))
                {
                    try
                    {
                        var htmlBody = $"""
                            <p>Estimado/a <strong>{patient.FirstName} {patient.LastName}</strong>,</p>
                            <p>Notamos que no pudo asistir a su cita programada para el <strong>{apt.ScheduledDate:dd/MM/yyyy HH:mm}</strong>.</p>
                            <p>Le invitamos a reprogramar su cita a la brevedad posible para continuar con su atención médica.</p>
                            <p>Gracias,<br/>Lova Salud</p>
                            """;
                        await email.SendAsync(patient.Email, "Cita perdida — Lova Salud", htmlBody, CancellationToken.None);
                        emailed = true;
                    }
                    catch { /* swallow */ }
                }

                if (!string.IsNullOrEmpty(patient.Phone))
                {
                    try
                    {
                        await sms.SendAsync(patient.Phone,
                            $"No te vimos en tu cita ({apt.ScheduledDate:dd/MM HH:mm}). Reprograma cuando puedas — Lova Salud.",
                            CancellationToken.None);
                        smsSent = true;
                    }
                    catch { /* swallow */ }
                }

                var channel = (emailed, smsSent) switch
                {
                    (true, true) => "Email+SMS",
                    (true, false) => "Email",
                    (false, true) => "SMS",
                    _ => "Manual",
                };

                await uow.NoShowOutreachLogs.AddAsync(new NoShowOutreachLog
                {
                    PatientId = patient.Id,
                    AppointmentId = apt.Id,
                    Channel = channel,
                    Notes = "Auto-outreach after appointment missed.",
                }, CancellationToken.None);
            }

            await uow.SaveChangesAsync(CancellationToken.None);
        }
    }

    /// <summary>Daily: invite recently finalized consults to a satisfaction survey.</summary>
    public async Task SendSatisfactionSurveysAsync()
    {
        var since = DateTime.UtcNow.AddHours(-26);
        var until = DateTime.UtcNow.AddHours(-22);

        var consults = await uow.Consults.FindAsync(
            c => c.Status == ConsultStatusEnum.Finished
              && c.FinishedAt >= since
              && c.FinishedAt < until,
            CancellationToken.None);

        foreach (var c in consults)
        {
            var existing = (await uow.SatisfactionSurveys
                .FindAsync(s => s.ConsultId == c.Id, CancellationToken.None))
                .FirstOrDefault();
            if (existing is not null) continue;

            var patient = await uow.Patients.GetByIdAsync(c.PatientId, CancellationToken.None);
            if (patient is null) continue;

            var token = Guid.NewGuid().ToString("N");
            await uow.SatisfactionSurveys.AddAsync(new SatisfactionSurvey
            {
                ConsultId = c.Id,
                PatientId = patient.Id,
                DoctorId = c.DoctorId,
                Token = token,
            }, CancellationToken.None);

            if (!string.IsNullOrEmpty(patient.Email))
            {
                try
                {
                    var body = $"""
                        <p>Hola <strong>{patient.FirstName}</strong>,</p>
                        <p>¿Cómo fue tu experiencia? Tu opinión nos ayuda a mejorar.</p>
                        <p>Toma menos de 1 minuto: <a href="https://portal.lovasalud.com/survey/{token}">responder encuesta</a></p>
                        <p>Gracias,<br/>Lova Salud</p>
                        """;
                    await email.SendAsync(patient.Email, "¿Cómo fue tu visita? — Lova Salud", body, CancellationToken.None);
                }
                catch { /* swallow */ }
            }
        }

        await uow.SaveChangesAsync(CancellationToken.None);
        if (consults.Any())
            logger.LogInformation("Queued {Count} satisfaction survey invitations.", consults.Count());
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

    /// <summary>Daily at 7:30 AM: email yesterday's revenue summary to clinic admin.</summary>
    public async Task SendScheduledReportsAsync()
    {
        var settings = (await uow.ClinicSettings.FindAsync(_ => true, CancellationToken.None)).FirstOrDefault();
        if (settings is null || !settings.EmailNotificationsEnabled || string.IsNullOrEmpty(settings.Email))
            return;

        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var payments = await db.Payments
            .Where(p => p.PaymentDate.Date == yesterday)
            .ToListAsync(CancellationToken.None);

        var totalRevenue = payments.Sum(p => p.Amount);
        var cashRevenue = payments.Where(p => p.Method == PaymentMethodEnum.Cash).Sum(p => p.Amount);
        var cardRevenue = payments.Where(p => p.Method == PaymentMethodEnum.CreditCard || p.Method == PaymentMethodEnum.DebitCard).Sum(p => p.Amount);
        var transferRevenue = payments.Where(p => p.Method == PaymentMethodEnum.BankTransfer).Sum(p => p.Amount);

        var invoices = await uow.Invoices.FindAsync(
            i => i.CreatedAt.Date == yesterday, CancellationToken.None);
        var paidInvoices = invoices.Count(i => i.Status == InvoiceStatusEnum.Paid);

        var html = $"""
            <h2>Reporte Diario — {yesterday:dd/MM/yyyy}</h2>
            <table border="1" cellpadding="6" style="border-collapse:collapse">
              <tr><td><strong>Total recaudado</strong></td><td>RD$ {totalRevenue:N2}</td></tr>
              <tr><td>Efectivo</td><td>RD$ {cashRevenue:N2}</td></tr>
              <tr><td>Tarjeta</td><td>RD$ {cardRevenue:N2}</td></tr>
              <tr><td>Transferencia</td><td>RD$ {transferRevenue:N2}</td></tr>
              <tr><td>Facturas generadas</td><td>{invoices.Count()}</td></tr>
              <tr><td>Facturas pagadas</td><td>{paidInvoices}</td></tr>
            </table>
            <p><em>Generado automáticamente por Lova Salud</em></p>
            """;

        try
        {
            await email.SendAsync(settings.Email, $"Reporte Diario {yesterday:dd/MM/yyyy} — Lova Salud", html, CancellationToken.None);
            logger.LogInformation("Sent daily revenue report for {Date}", yesterday);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send scheduled daily report");
        }
    }
}
