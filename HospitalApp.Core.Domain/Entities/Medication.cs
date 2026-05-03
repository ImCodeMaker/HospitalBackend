using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class Medication : SharedEntity
{
    public required string GenericName { get; set; }
    public string? BrandName { get; set; }
    public string? AtcCode { get; set; }
    public required MedicationPresentationEnum Presentation { get; set; }
    public required string Strength { get; set; } // e.g. "500mg", "250mg/5mL"
    public required string UnitOfMeasure { get; set; } // tablet, mL, vial, etc.

    public int CurrentStock { get; set; }
    public int MinimumStockThreshold { get; set; }
    public int ReorderQuantity { get; set; }

    public string? StorageLocation { get; set; }
    public bool RequiresRefrigeration { get; set; }
    public bool IsControlledSubstance { get; set; }
    public string? ControlledSubstanceClass { get; set; }

    public string? Supplier { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }

    public DateTime? EarliestExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsLowStock => CurrentStock <= MinimumStockThreshold && CurrentStock > 0;
    public bool IsOutOfStock => CurrentStock == 0;
    public bool IsExpired => EarliestExpirationDate.HasValue && EarliestExpirationDate.Value < DateTime.UtcNow;
    public bool IsExpiringSoon => EarliestExpirationDate.HasValue
        && EarliestExpirationDate.Value > DateTime.UtcNow
        && EarliestExpirationDate.Value <= DateTime.UtcNow.AddDays(30);

    public ICollection<StockTransaction> StockTransactions { get; set; } = [];
}
