using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class CashTransaction : SharedEntity
{
    public required Guid ShiftId { get; set; }
    public required Guid CreatedByUserId { get; set; }
    public Guid? InvoiceId { get; set; }
    public required CashTransactionTypeEnum Type { get; set; }
    public required decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? ReceiptPath { get; set; }
    public bool RequiresAdminApproval { get; set; }
    public bool IsApproved { get; set; } = true;
    public Guid? ApprovedByUserId { get; set; }

    public CajaShift? Shift { get; set; }
    public Invoice? Invoice { get; set; }
}
