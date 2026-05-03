namespace HospitalApp.Core.Application.Features.Patients.DTOs;

public class PatientTimelineItemDto
{
    public string Category { get; init; } = string.Empty; // "Consult","Payment","LabOrder","Appointment"
    public Guid EntityId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Detail { get; init; }
    public DateTime OccurredAt { get; init; }
}
