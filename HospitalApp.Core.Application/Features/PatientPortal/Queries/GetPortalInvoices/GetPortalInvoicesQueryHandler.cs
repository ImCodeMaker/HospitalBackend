using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.PatientPortal.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalInvoices;

public class GetPortalInvoicesQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetPortalInvoicesQuery, Result<List<PortalInvoiceDto>>>
{
    public async Task<Result<List<PortalInvoiceDto>>> Handle(GetPortalInvoicesQuery query, CancellationToken ct)
    {
        var invoices = await uow.Invoices.FindAsync(i => i.PatientId == query.PatientId, ct);

        var result = invoices
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new PortalInvoiceDto(
                i.Id,
                i.InvoiceNumber,
                i.CreatedAt,
                i.Status.ToString(),
                i.TotalAmount,
                i.PatientResponsibilityAmount,
                i.PaidAmount,
                i.BalanceDue,
                i.PaidAt
            ))
            .ToList();

        return Result<List<PortalInvoiceDto>>.Success(result);
    }
}
