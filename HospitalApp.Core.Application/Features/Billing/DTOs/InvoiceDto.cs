namespace HospitalApp.Core.Application.Features.Billing.DTOs;

public record InvoiceDto
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string? Ncf { get; init; }
    public string? NcfType { get; init; }
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public Guid ConsultId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? InsuranceDenialReason { get; init; }
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal InsuranceCoverageAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PatientResponsibilityAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal BalanceDue { get; init; }
    public string? Notes { get; init; }
    public DateTime? PaidAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<InvoiceLineItemDto> LineItems { get; init; } = [];
    public List<PaymentDto> Payments { get; init; } = [];
}

public class InvoiceLineItemDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal InsuranceCoverageAmount { get; init; }
    public decimal PatientAmount { get; init; }
}

public class PaymentDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string Method { get; init; } = string.Empty;
    public string? ReferenceNumber { get; init; }
    public DateTime PaymentDate { get; init; }
}
