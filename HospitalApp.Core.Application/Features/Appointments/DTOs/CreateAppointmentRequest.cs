using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.Appointments.DTOs;

public class CreateAppointmentRequest
{
    public Guid PatientId { get; init; }
    public Guid AssignedDoctorId { get; init; }
    public Guid? OriginatingConsultId { get; init; }
    public DateTime ScheduledDate { get; init; }
    public int DurationMinutes { get; init; } = 30;
    public AppointmentTypeEnum Type { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}
