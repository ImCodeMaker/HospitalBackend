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

        var transactionType = MapToCashTransactionType(req.Method);
        if (transactionType is null)
            return Result<Guid>.Failure("Insurance or mixed payments are not posted to caja automatically. Use a supported payment method or split mixed payments.", 400);

        var shift = await uow.CajaShifts.FirstOrDefaultAsync(s => s.IsOpen, ct);
        if (shift is null)
            return Result<Guid>.Failure("No open caja shift. Open a caja shift before recording this payment.", 409);

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

        var cashTransaction = new CashTransaction
        {
            ShiftId = shift.Id,
            CreatedByUserId = command.ReceivedByUserId,
            InvoiceId = invoice.Id,
            Type = transactionType.Value,
            Amount = req.Amount,
            Description = $"Invoice {invoice.InvoiceNumber} payment",
            IsApproved = true,
        };

        invoice.PaidAmount += req.Amount;
        invoice.Status = invoice.PaidAmount >= invoice.PatientResponsibilityAmount
            ? InvoiceStatusEnum.Paid
            : InvoiceStatusEnum.PartiallyPaid;

        if (invoice.Status == InvoiceStatusEnum.Paid)
            invoice.PaidAt = DateTime.UtcNow;

        invoice.UpdatedAt = DateTime.UtcNow;

        await uow.Payments.AddAsync(payment, ct);
        await uow.CashTransactions.AddAsync(cashTransaction, ct);
        uow.Invoices.Update(invoice);
        await uow.SaveChangesAsync(ct);
        await notifier.NotifyBillingChangedAsync(ct);
        await notifier.NotifyCajaChangedAsync(ct);

        return Result<Guid>.Created(payment.Id);
    }

    private static CashTransactionTypeEnum? MapToCashTransactionType(PaymentMethodEnum method) =>
        method switch
        {
            PaymentMethodEnum.Cash => CashTransactionTypeEnum.PaymentCash,
            PaymentMethodEnum.CreditCard or PaymentMethodEnum.DebitCard => CashTransactionTypeEnum.PaymentCard,
            PaymentMethodEnum.BankTransfer => CashTransactionTypeEnum.BankTransfer,
            _ => null,
        };
}
