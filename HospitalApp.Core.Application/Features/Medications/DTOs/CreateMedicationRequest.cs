using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.Medications.DTOs;

public class CreateMedicationRequest
{
    public string GenericName { get; init; } = string.Empty;
    public string? BrandName { get; init; }
    public string? AtcCode { get; init; }
    public MedicationPresentationEnum Presentation { get; init; }
    public string Strength { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public int InitialStock { get; init; }
    public int MinimumStockThreshold { get; init; }
    public int ReorderQuantity { get; init; }
    public string? StorageLocation { get; init; }
    public bool RequiresRefrigeration { get; init; }
    public bool IsControlledSubstance { get; init; }
    public string? ControlledSubstanceClass { get; init; }
    public string? Supplier { get; init; }
    public decimal CostPrice { get; init; }
    public decimal SalePrice { get; init; }
    public DateTime? EarliestExpirationDate { get; init; }
    public string? BatchNumber { get; init; }
    public string? Notes { get; init; }
}
