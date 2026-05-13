namespace HospitalApp.Core.Domain.Entities;

/// <summary>
/// Tracks each no-show outreach attempt against a patient.
/// Used to flag repeat-offenders (e.g. 3+ no-shows in 90 days).
/// </summary>
public class NoShowOutreachLog : SharedEntity
{
    public required Guid PatientId { get; set; }
    public required Guid AppointmentId { get; set; }
    public DateTime ContactedAt { get; set; } = DateTime.UtcNow;
    public required string Channel { get; set; } // "Email" | "SMS" | "Manual"
    public string? Notes { get; set; }
    public bool PatientResponded { get; set; }

    public Patient? Patient { get; set; }
    public Appointment? Appointment { get; set; }
}
