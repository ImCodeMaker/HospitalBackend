namespace HospitalApp.Core.Domain.Enums;

public enum InvoiceStatusEnum
{
    Draft,
    AwaitingPayment,
    PartiallyPaid,
    Paid,
    PendingInsurance,
    RequiresCollection,
    Cancelled,
    Refunded
}
