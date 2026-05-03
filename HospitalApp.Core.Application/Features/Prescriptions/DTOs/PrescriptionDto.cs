namespace HospitalApp.Core.Application.Features.Prescriptions.DTOs;

public class PrescriptionDto
{
    public Guid Id { get; init; }
    public Guid ConsultId { get; init; }
    public string DrugName { get; init; } = string.Empty;
    public string Presentation { get; init; } = string.Empty;
    public string Dosage { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public string RouteOfAdministration { get; init; } = string.Empty;
    public int DurationDays { get; init; }
    public int QuantityToDispense { get; init; }
    public string? SpecialInstructions { get; init; }
    public bool IsDispensed { get; init; }
    public DateTime? DispensedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
