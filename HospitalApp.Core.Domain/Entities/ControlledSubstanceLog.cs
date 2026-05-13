namespace HospitalApp.Core.Domain.Entities;

/// <summary>
/// Immutable audit ledger for every movement of a controlled substance.
/// Separate from <see cref="StockTransaction"/> for DEA / regulatory inspection.
/// Insert-only — never updated or deleted.
/// </summary>
public class ControlledSubstanceLog : SharedEntity
{
    public required Guid MedicationId { get; set; }
    public required Guid PerformedByUserId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? PrescriptionId { get; set; }
    public required string Action { get; set; } // Received / Dispensed / WriteOff / Adjustment
    public required int Quantity { get; set; }
    public required int StockBefore { get; set; }
    public required int StockAfter { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? WitnessUserId { get; set; }
    public string? Reason { get; set; }

    public Medication? Medication { get; set; }
    public Patient? Patient { get; set; }
}
