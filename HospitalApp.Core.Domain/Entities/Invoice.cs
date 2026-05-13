using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class Invoice : SharedEntity
{
    public required Guid PatientId { get; set; }
    public required Guid ConsultId { get; set; }
    public required Guid CreatedByUserId { get; set; }

    public required string InvoiceNumber { get; set; }

    /// <summary>DGII fiscal receipt number (Número de Comprobante Fiscal).</summary>
    public string? Ncf { get; set; }

    /// <summary>NCF type used at issuance (B01/B02/...).</summary>
    public NcfTypeEnum? NcfType { get; set; }

    public InvoiceStatusEnum Status { get; set; } = InvoiceStatusEnum.AwaitingPayment;

    /// <summary>Set when insurance claim is denied. Status is then RequiresCollection.</summary>
    public string? InsuranceDenialReason { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; } // ITBIS if applicable
    public decimal InsuranceCoverageAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PatientResponsibilityAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue => PatientResponsibilityAmount - PaidAmount;

    public string? Notes { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? DueDate { get; set; }

    public Patient? Patient { get; set; }
    public Consult? Consult { get; set; }
    public ICollection<InvoiceLineItem> LineItems { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
