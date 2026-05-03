using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class Payment : SharedEntity
{
    public required Guid InvoiceId { get; set; }
    public required Guid ReceivedByUserId { get; set; }

    public required decimal Amount { get; set; }
    public required PaymentMethodEnum Method { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public Invoice? Invoice { get; set; }
}
