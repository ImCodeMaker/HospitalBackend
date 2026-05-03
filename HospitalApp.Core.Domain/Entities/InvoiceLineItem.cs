using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class InvoiceLineItem : SharedEntity
{
    public required Guid InvoiceId { get; set; }

    public required InvoiceLineItemTypeEnum Type { get; set; }
    public required string Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal DiscountAmount { get; set; }
    public decimal InsuranceCoverageAmount { get; set; }
    public decimal PatientAmount => (UnitPrice * Quantity) - DiscountAmount - InsuranceCoverageAmount;

    public Guid? ReferenceId { get; set; } // FK to LabOrder, Medication, etc.

    public Invoice? Invoice { get; set; }
}
