using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Billing.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
    {
        var invoice = await uow.Invoices.GetByIdAsync(query.InvoiceId, ct);
        if (invoice is null)
            return Result<InvoiceDto>.NotFound("Invoice not found.");

        return Result<InvoiceDto>.Success(mapper.Map<InvoiceDto>(invoice));
    }
}
