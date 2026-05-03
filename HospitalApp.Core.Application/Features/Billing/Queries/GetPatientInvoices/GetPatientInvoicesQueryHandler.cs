using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Billing.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Queries.GetPatientInvoices;

public class GetPatientInvoicesQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetPatientInvoicesQuery, Result<PaginatedResult<InvoiceDto>>>
{
    public async Task<Result<PaginatedResult<InvoiceDto>>> Handle(GetPatientInvoicesQuery query, CancellationToken ct)
    {
        var all = await uow.Invoices.FindAsync(i =>
            i.PatientId == query.PatientId &&
            (!query.Status.HasValue || i.Status == query.Status), ct);

        var ordered = all.OrderByDescending(i => i.CreatedAt).ToList();
        var items = ordered.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        return Result<PaginatedResult<InvoiceDto>>.Success(
            PaginatedResult<InvoiceDto>.Create(
                mapper.Map<IReadOnlyList<InvoiceDto>>(items), ordered.Count, query.PageNumber, query.PageSize));
    }
}
