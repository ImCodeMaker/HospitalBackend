namespace HospitalApp.Core.Application.Features.Appointments.DTOs;

public class AppointmentDto
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public Guid AssignedDoctorId { get; init; }
    public string DoctorName { get; init; } = string.Empty;
    public Guid? OriginatingConsultId { get; init; }
    public DateTime ScheduledDate { get; init; }
    public int DurationMinutes { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public string? Notes { get; init; }
    public bool ReminderSent { get; init; }
    public DateTime CreatedAt { get; init; }
}
