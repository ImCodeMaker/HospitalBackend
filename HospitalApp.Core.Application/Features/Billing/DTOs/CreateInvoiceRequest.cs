using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.Billing.DTOs;

public class CreateInvoiceRequest
{
    public Guid ConsultId { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public string? Notes { get; init; }
    /// <summary>NCF type to assign. Defaults to Consumo (B02) when null.</summary>
    public NcfTypeEnum? NcfType { get; init; }
    public List<CreateLineItemRequest> LineItems { get; init; } = [];
}

public class CreateLineItemRequest
{
    public InvoiceLineItemTypeEnum Type { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; } = 1;
    public decimal DiscountAmount { get; init; }
    public decimal InsuranceCoverageAmount { get; init; }
    public Guid? ReferenceId { get; init; }
}

public class ProcessPaymentRequest
{
    public decimal Amount { get; init; }
    public PaymentMethodEnum Method { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Notes { get; init; }
}
