using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Billing.DTOs;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Queries.GetInvoices;

public class GetInvoicesQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetInvoicesQuery, Result<PaginatedResult<InvoiceDto>>>
{
    public async Task<Result<PaginatedResult<InvoiceDto>>> Handle(GetInvoicesQuery query, CancellationToken ct)
    {
        InvoiceStatusEnum? statusFilter = null;
        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<InvoiceStatusEnum>(query.Status, out var parsed))
            statusFilter = parsed;

        var all = await uow.Invoices.FindAsync(i =>
            (!query.PatientId.HasValue || i.PatientId == query.PatientId) &&
            (!statusFilter.HasValue || i.Status == statusFilter.Value) &&
            (!query.From.HasValue || i.CreatedAt >= query.From) &&
            (!query.To.HasValue || i.CreatedAt <= query.To), ct);

        var ordered = all.OrderByDescending(i => i.CreatedAt).ToList();
        var total = ordered.Count;
        var items = ordered.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        return Result<PaginatedResult<InvoiceDto>>.Success(
            PaginatedResult<InvoiceDto>.Create(
                mapper.Map<IReadOnlyList<InvoiceDto>>(items), total, query.PageNumber, query.PageSize));
    }
}
