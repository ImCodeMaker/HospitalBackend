using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Commands.SubmitInsuranceClaim;

public class SubmitInsuranceClaimCommandHandler(IUnitOfWork uow)
    : IRequestHandler<SubmitInsuranceClaimCommand, Result>
{
    public async Task<Result> Handle(SubmitInsuranceClaimCommand command, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(command.InvoiceId, ct);
        if (invoice is null)
            return Result.NotFound("Invoice not found.");

        if (invoice.Status != InvoiceStatusEnum.AwaitingPayment &&
            invoice.Status != InvoiceStatusEnum.PartiallyPaid)
            return Result.Failure($"Cannot submit insurance claim for invoice with status '{invoice.Status}'.", 409);

        invoice.Status = InvoiceStatusEnum.PendingInsurance;
        invoice.Notes = string.IsNullOrEmpty(invoice.Notes)
            ? $"Insurance claim submitted: {command.ClaimReferenceNumber}"
            : $"{invoice.Notes}\nInsurance claim submitted: {command.ClaimReferenceNumber}";
        invoice.UpdatedAt = DateTime.UtcNow;

        uow.Invoices.Update(invoice);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
