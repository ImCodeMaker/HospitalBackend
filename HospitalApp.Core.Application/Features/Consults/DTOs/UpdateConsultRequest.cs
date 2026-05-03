namespace HospitalApp.Core.Application.Features.Consults.DTOs;

public class UpdateConsultRequest
{
    public decimal? WeightKg { get; init; }
    public decimal? HeightCm { get; init; }
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
    public string? ReferralNotes { get; init; }

    /// <summary>Serialized JSON object with specialty-specific fields.</summary>
    public string? SpecialtyData { get; init; }
}
