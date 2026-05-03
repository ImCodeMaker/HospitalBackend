using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Commands.ResolveInsuranceClaim;

public class ResolveInsuranceClaimCommandHandler(IUnitOfWork uow)
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
            invoice.Status = invoice.PaidAmount >= invoice.PatientResponsibilityAmount
                ? InvoiceStatusEnum.Paid
                : InvoiceStatusEnum.PartiallyPaid;

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
            invoice.Status = invoice.PaidAmount > 0
                ? InvoiceStatusEnum.PartiallyPaid
                : InvoiceStatusEnum.RequiresCollection;

            var deniedNote = "Insurance claim denied.";
            if (!string.IsNullOrEmpty(command.Notes))
                deniedNote += $" {command.Notes}";

            invoice.Notes = string.IsNullOrEmpty(invoice.Notes)
                ? deniedNote
                : $"{invoice.Notes}\n{deniedNote}";
        }

        invoice.UpdatedAt = DateTime.UtcNow;

        uow.Invoices.Update(invoice);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
