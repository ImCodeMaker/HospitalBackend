namespace HospitalApp.Core.Application.Features.Consults.DTOs;

public class CreateConsultRequest
{
    public Guid PatientId { get; init; }
    public Guid SpecialtyId { get; init; }

    // Vitals (optional at creation)
    public decimal? WeightKg { get; init; }
    public decimal? HeightCm { get; init; }
    public int? BpSystolic { get; init; }
    public int? BpDiastolic { get; init; }
    public int? HeartRate { get; init; }
    public decimal? TemperatureCelsius { get; init; }
    public decimal? O2Saturation { get; init; }
    public int? RespiratoryRate { get; init; }

    public string? ChiefComplaint { get; init; }
}
