namespace HospitalApp.Core.Application.Features.Reports.DTOs;

public record DailyRevenueSummaryDto(
    DateTime Date,
    decimal TotalRevenue,
    decimal CashRevenue,
    decimal CardRevenue,
    decimal TransferRevenue,
    decimal InsuranceRevenue,
    int TotalInvoices,
    int PaidInvoices
);

public record AccountsReceivableDto(
    Guid InvoiceId,
    string InvoiceNumber,
    string PatientName,
    decimal BalanceDue,
    DateTime InvoiceDate,
    int DaysOutstanding,
    string AgingBucket // 0-30, 31-60, 61-90, 90+
);

public record ConsultationVolumeDto(
    DateTime Date,
    int TotalConsults,
    int FinishedConsults,
    int InProgressConsults
);

public record InventoryReportItemDto(
    Guid Id,
    string GenericName,
    string? BrandName,
    int CurrentStock,
    int MinimumStockThreshold,
    decimal SalePrice,
    bool IsLowStock,
    bool IsOutOfStock,
    bool IsExpiringSoon,
    bool IsExpired,
    DateTime? EarliestExpirationDate
);
