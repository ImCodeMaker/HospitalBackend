using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class StockTransaction : SharedEntity
{
    public required Guid MedicationId { get; set; }
    public required Guid PerformedByUserId { get; set; }
    public Guid? ConsultId { get; set; } // set when type = PrescriptionDispensed

    public required StockTransactionTypeEnum Type { get; set; }
    public required int Quantity { get; set; } // positive = added, negative = removed
    public required int StockBefore { get; set; }
    public required int StockAfter { get; set; }
    public string? Reason { get; set; }
    public string? BatchNumber { get; set; }

    public Medication? Medication { get; set; }
}
