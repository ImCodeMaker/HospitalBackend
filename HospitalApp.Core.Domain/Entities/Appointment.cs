using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class Appointment : SharedEntity
{
    public required Guid PatientId { get; set; }
    public required Guid AssignedDoctorId { get; set; }
    public Guid? OriginatingConsultId { get; set; }
    public Guid? ScheduledByUserId { get; set; }

    public required DateTime ScheduledDate { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public required AppointmentTypeEnum Type { get; set; }
    public AppointmentStatusEnum Status { get; set; } = AppointmentStatusEnum.Scheduled;
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    public bool ReminderSent { get; set; }
    public DateTime? ReminderSentAt { get; set; }
    /// <summary>True once the 24-hour reminder has been delivered.</summary>
    public bool Reminder24hSent { get; set; }
    /// <summary>True once the 2-hour reminder has been delivered.</summary>
    public bool Reminder2hSent { get; set; }
    public bool NoShowNotificationSent { get; set; }

    public Patient? Patient { get; set; }
    public Consult? OriginatingConsult { get; set; }
}
