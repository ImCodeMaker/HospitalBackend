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
            invoice.Status != InvoiceStatusEnum.PartiallyPaid &&
            invoice.Status != InvoiceStatusEnum.RequiresCollection)
            return Result.Failure($"Cannot submit insurance claim for invoice with status '{invoice.Status}'.", 409);

        // Resubmission after denial: clear the denial reason and reset insurance coverage.
        var isResubmission = invoice.Status == InvoiceStatusEnum.RequiresCollection
                          && !string.IsNullOrEmpty(invoice.InsuranceDenialReason);
        if (isResubmission)
        {
            invoice.InsuranceDenialReason = null;
        }

        invoice.Status = InvoiceStatusEnum.PendingInsurance;
        var prefix = isResubmission ? "Insurance claim RESUBMITTED" : "Insurance claim submitted";
        invoice.Notes = string.IsNullOrEmpty(invoice.Notes)
            ? $"{prefix}: {command.ClaimReferenceNumber}"
            : $"{invoice.Notes}\n{prefix}: {command.ClaimReferenceNumber}";
        invoice.UpdatedAt = DateTime.UtcNow;

        uow.Invoices.Update(invoice);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
