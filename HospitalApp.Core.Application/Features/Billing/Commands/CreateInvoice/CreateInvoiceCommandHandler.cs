using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Commands.CreateInvoice;

public class CreateInvoiceCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier, INcfService ncf)
    : IRequestHandler<CreateInvoiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInvoiceCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var consult = await uow.Consults.GetByIdAsync(req.ConsultId, ct);
        if (consult is null)
            return Result<Guid>.NotFound("Consult not found.");

        if (consult.Status != ConsultStatusEnum.Finished)
            return Result<Guid>.Failure("Consult must be finalized before billing.", 409);

        var existingInvoice = await uow.Invoices.FirstOrDefaultAsync(
            i => i.ConsultId == req.ConsultId, ct);
        if (existingInvoice is not null)
            return Result<Guid>.Failure("Invoice already exists for this consult.", 409);

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var ncfType = req.NcfType ?? NcfTypeEnum.Consumo;
        var ncfNumber = await ncf.ReserveNextAsync(ncfType, ct);
        if (ncfNumber is null)
            return Result<Guid>.Failure(
                $"NCF range for type {ncfType.GetPrefix()} is exhausted or expired. Configure a new range before issuing more invoices.",
                409);

        var subtotal = req.LineItems.Sum(li => li.UnitPrice * li.Quantity);
        var totalDiscount = req.DiscountAmount + req.LineItems.Sum(li => li.DiscountAmount);
        var totalInsurance = req.LineItems.Sum(li => li.InsuranceCoverageAmount);
        var total = subtotal - totalDiscount + req.TaxAmount;
        var patientResponsibility = total - totalInsurance;

        var invoice = new Invoice
        {
            PatientId = consult.PatientId,
            ConsultId = req.ConsultId,
            CreatedByUserId = command.CreatedByUserId,
            InvoiceNumber = invoiceNumber,
            Ncf = ncfNumber,
            NcfType = ncfType,
            Status = InvoiceStatusEnum.AwaitingPayment,
            Subtotal = subtotal,
            DiscountAmount = totalDiscount,
            TaxAmount = req.TaxAmount,
            InsuranceCoverageAmount = totalInsurance,
            TotalAmount = total,
            PatientResponsibilityAmount = patientResponsibility,
            PaidAmount = 0,
            Notes = req.Notes,
        };

        var lineItems = req.LineItems.Select(li => new InvoiceLineItem
        {
            InvoiceId = invoice.Id,
            Type = li.Type,
            Description = li.Description,
            UnitPrice = li.UnitPrice,
            Quantity = li.Quantity,
            DiscountAmount = li.DiscountAmount,
            InsuranceCoverageAmount = li.InsuranceCoverageAmount,
            ReferenceId = li.ReferenceId,
        }).ToList();
        invoice.LineItems = lineItems;

        await uow.Invoices.AddAsync(invoice, ct);
        await uow.SaveChangesAsync(ct);
        await notifier.NotifyBillingChangedAsync(ct);

        return Result<Guid>.Created(invoice.Id);
    }
}
