using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class MedicalPrescription : SharedEntity
{
    public required Guid ConsultId { get; set; }
    public required Guid PrescribedByDoctorId { get; set; }

    public required string DrugName { get; set; }
    public string? MedicationId { get; set; } // FK to inventory if in stock
    public required string Presentation { get; set; }
    public required string Dosage { get; set; }
    public required string Frequency { get; set; }
    public required string RouteOfAdministration { get; set; }
    public int DurationDays { get; set; }
    public int QuantityToDispense { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool IsDispensed { get; set; }
    public DateTime? DispensedAt { get; set; }

    public Consult? Consult { get; set; }
}
