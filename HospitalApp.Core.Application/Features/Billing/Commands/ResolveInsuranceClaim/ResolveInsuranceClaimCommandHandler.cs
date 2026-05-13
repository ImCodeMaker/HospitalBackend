using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Commands.ResolveInsuranceClaim;

public class ResolveInsuranceClaimCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier)
    : IRequestHandler<ResolveInsuranceClaimCommand, Result>
{
    public async Task<Result> Handle(ResolveInsuranceClaimCommand command, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(command.InvoiceId, ct);
        if (invoice is null)
            return Result.NotFound("Invoice not found.");

        if (invoice.Status != InvoiceStatusEnum.PendingInsurance)
            return Result.Failure("Invoice is not in PendingInsurance status.", 409);

        if (command.Approved)
        {
            var amount = command.ApprovedAmount ?? 0m;
            invoice.PaidAmount += amount;

            // If insurance covered less than originally estimated, shift the gap to patient.
            var unpaidInsurancePortion = invoice.InsuranceCoverageAmount - amount;
            if (unpaidInsurancePortion > 0)
            {
                invoice.PatientResponsibilityAmount += unpaidInsurancePortion;
                invoice.InsuranceCoverageAmount = amount;
            }

            invoice.Status = invoice.PaidAmount >= invoice.PatientResponsibilityAmount
                ? InvoiceStatusEnum.Paid
                : (invoice.PaidAmount > 0 ? InvoiceStatusEnum.PartiallyPaid : InvoiceStatusEnum.RequiresCollection);

            if (invoice.Status == InvoiceStatusEnum.Paid)
                invoice.PaidAt = DateTime.UtcNow;

            var approvedNote = $"Insurance claim approved. Amount: {amount:C}.";
            if (!string.IsNullOrEmpty(command.Notes))
                approvedNote += $" {command.Notes}";

            invoice.Notes = string.IsNullOrEmpty(invoice.Notes)
                ? approvedNote
                : $"{invoice.Notes}\n{approvedNote}";
        }
        else
        {
            // Full denial: insurance portion becomes patient's responsibility.
            invoice.PatientResponsibilityAmount += invoice.InsuranceCoverageAmount;
            invoice.InsuranceCoverageAmount = 0;
            invoice.InsuranceDenialReason = !string.IsNullOrEmpty(command.Notes)
                ? command.Notes
                : "Insurance denied without specific reason provided.";

            invoice.Status = invoice.PaidAmount >= invoice.PatientResponsibilityAmount
                ? InvoiceStatusEnum.Paid
                : (invoice.PaidAmount > 0 ? InvoiceStatusEnum.PartiallyPaid : InvoiceStatusEnum.RequiresCollection);

            var deniedNote = $"Insurance claim DENIED: {invoice.InsuranceDenialReason}";
            invoice.Notes = string.IsNullOrEmpty(invoice.Notes)
                ? deniedNote
                : $"{invoice.Notes}\n{deniedNote}";
        }

        invoice.UpdatedAt = DateTime.UtcNow;

        uow.Invoices.Update(invoice);
        await uow.SaveChangesAsync(ct);
        await notifier.NotifyBillingChangedAsync(ct);

        return Result.Success();
    }
}
