using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier)
    : IRequestHandler<ProcessPaymentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ProcessPaymentCommand command, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(command.InvoiceId, ct);
        if (invoice is null)
            return Result<Guid>.NotFound("Invoice not found.");

        if (invoice.Status == InvoiceStatusEnum.Paid)
            return Result<Guid>.Failure("Invoice already fully paid.", 409);

        if (invoice.Status == InvoiceStatusEnum.Cancelled)
            return Result<Guid>.Failure("Cannot pay a cancelled invoice.", 409);

        var req = command.Request;
        if (req.Amount <= 0)
            return Result<Guid>.Failure("Payment amount must be positive.");

        if (req.Amount > invoice.BalanceDue)
            return Result<Guid>.Failure($"Amount exceeds balance due ({invoice.BalanceDue:C}).");

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            ReceivedByUserId = command.ReceivedByUserId,
            Amount = req.Amount,
            Method = req.Method,
            ReferenceNumber = req.ReferenceNumber,
            Notes = req.Notes,
            PaymentDate = DateTime.UtcNow,
        };

        invoice.PaidAmount += req.Amount;
        invoice.Status = invoice.PaidAmount >= invoice.PatientResponsibilityAmount
            ? InvoiceStatusEnum.Paid
            : InvoiceStatusEnum.PartiallyPaid;

        if (invoice.Status == InvoiceStatusEnum.Paid)
            invoice.PaidAt = DateTime.UtcNow;

        invoice.UpdatedAt = DateTime.UtcNow;

        await uow.Payments.AddAsync(payment, ct);
        uow.Invoices.Update(invoice);
        await uow.SaveChangesAsync(ct);
        await notifier.NotifyBillingChangedAsync(ct);

        return Result<Guid>.Created(payment.Id);
    }
}
