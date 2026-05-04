using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Queries.GetConsults;

public class GetConsultsQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetConsultsQuery, Result<PaginatedResult<ConsultDto>>>
{
    public async Task<Result<PaginatedResult<ConsultDto>>> Handle(GetConsultsQuery query, CancellationToken ct)
    {
        ConsultStatusEnum? statusFilter = null;
        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<ConsultStatusEnum>(query.Status, out var parsed))
            statusFilter = parsed;

        var all = await uow.Consults.FindAsync(
            c => !statusFilter.HasValue || c.Status == statusFilter.Value, ct);

        var ordered = all.OrderByDescending(c => c.CreatedAt).ToList();
        var total = ordered.Count;
        var items = ordered.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        return Result<PaginatedResult<ConsultDto>>.Success(
            PaginatedResult<ConsultDto>.Create(
                mapper.Map<IReadOnlyList<ConsultDto>>(items), total, query.PageNumber, query.PageSize));
    }
}
