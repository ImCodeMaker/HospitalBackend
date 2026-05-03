using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.Consults.DTOs;

public class ConsultDto
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public Guid SpecialtyId { get; init; }
    public string SpecialtyName { get; init; } = string.Empty;
    public Guid DoctorId { get; init; }
    public string DoctorName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    // Vitals
    public decimal? WeightKg { get; init; }
    public decimal? HeightCm { get; init; }
    public decimal? Bmi { get; init; }
    public int? BpSystolic { get; init; }
    public int? BpDiastolic { get; init; }
    public int? HeartRate { get; init; }
    public decimal? TemperatureCelsius { get; init; }
    public decimal? O2Saturation { get; init; }
    public int? RespiratoryRate { get; init; }

    public string? ChiefComplaint { get; init; }
    public string? ClinicalObservations { get; init; }
    public string? DiagnosisCodes { get; init; }
    public string? DiagnosisDescription { get; init; }
    public string? TreatmentPlan { get; init; }
    public string? SpecialtyData { get; init; } // raw JSON

    public DateTime? StartedAt { get; init; }
    public DateTime? FinishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
