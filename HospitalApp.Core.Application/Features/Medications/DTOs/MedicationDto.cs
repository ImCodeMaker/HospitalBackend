namespace HospitalApp.Core.Application.Features.Medications.DTOs;

public class MedicationDto
{
    public Guid Id { get; init; }
    public string GenericName { get; init; } = string.Empty;
    public string? BrandName { get; init; }
    public string Presentation { get; init; } = string.Empty;
    public string Strength { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public int CurrentStock { get; init; }
    public int MinimumStockThreshold { get; init; }
    public decimal SalePrice { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }
    public bool IsExpired { get; init; }
    public bool IsExpiringSoon { get; init; }
    public bool RequiresRefrigeration { get; init; }
    public bool IsControlledSubstance { get; init; }
    public DateTime? EarliestExpirationDate { get; init; }
    public string? StorageLocation { get; init; }
    public string? Supplier { get; init; }
    public bool IsActive { get; init; }
}
