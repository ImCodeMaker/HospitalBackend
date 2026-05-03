using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier, IEmailService email, ISmsService sms)
    : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var patient = await uow.Patients.GetByIdAsync(req.PatientId, ct);
        if (patient is null)
            return Result<Guid>.NotFound("Patient not found.");

        // Conflict check: same doctor, overlapping time slot
        var existing = await uow.Appointments.FindAsync(a =>
            a.AssignedDoctorId == req.AssignedDoctorId &&
            a.Status != AppointmentStatusEnum.Cancelled &&
            a.Status != AppointmentStatusEnum.Rescheduled &&
            a.ScheduledDate < req.ScheduledDate.AddMinutes(req.DurationMinutes) &&
            a.ScheduledDate.AddMinutes(a.DurationMinutes) > req.ScheduledDate, ct);

        if (existing.Any())
            return Result<Guid>.Failure("Doctor already has an appointment in this time slot.", 409);

        var appointment = new Appointment
        {
            PatientId = req.PatientId,
            AssignedDoctorId = req.AssignedDoctorId,
            OriginatingConsultId = req.OriginatingConsultId,
            ScheduledByUserId = command.ScheduledByUserId,
            ScheduledDate = req.ScheduledDate,
            DurationMinutes = req.DurationMinutes,
            Type = req.Type,
            Reason = req.Reason,
            Notes = req.Notes,
        };

        await uow.Appointments.AddAsync(appointment, ct);
        await uow.SaveChangesAsync(ct);
        await notifier.NotifyAppointmentChangedAsync(ct);

        if (!string.IsNullOrEmpty(patient.Email))
        {
            try
            {
                var htmlBody = $"""
                    <p>Estimado/a <strong>{patient.FirstName} {patient.LastName}</strong>,</p>
                    <p>Su cita ha sido confirmada con los siguientes detalles:</p>
                    <ul>
                        <li><strong>Fecha y hora:</strong> {appointment.ScheduledDate:dd/MM/yyyy HH:mm}</li>
                        <li><strong>Tipo:</strong> {appointment.Type}</li>
                        <li><strong>Duración:</strong> {appointment.DurationMinutes} minutos</li>
                    </ul>
                    <p>Si necesita reprogramar, por favor contáctenos con anticipación.</p>
                    <p>Gracias,<br/>Lova Salud</p>
                    """;

                await email.SendAsync(patient.Email, "Cita confirmada — Lova Salud", htmlBody, ct);
            }
            catch
            {
                // notification failure must not block operation
            }
        }

        if (!string.IsNullOrEmpty(patient.Phone))
        {
            try
            {
                await sms.SendAsync(patient.Phone,
                    $"Cita confirmada: {appointment.ScheduledDate:dd/MM/yyyy HH:mm}. Lova Salud.", ct);
            }
            catch
            {
                // notification failure must not block operation
            }
        }

        return Result<Guid>.Created(appointment.Id);
    }
}
