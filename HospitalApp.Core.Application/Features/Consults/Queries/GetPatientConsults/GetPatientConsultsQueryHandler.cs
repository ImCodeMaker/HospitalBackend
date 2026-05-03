using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Queries.GetPatientConsults;

public class GetPatientConsultsQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetPatientConsultsQuery, Result<PaginatedResult<ConsultDto>>>
{
    public async Task<Result<PaginatedResult<ConsultDto>>> Handle(GetPatientConsultsQuery query, CancellationToken ct)
    {
        var all = await uow.Consults.FindAsync(c => c.PatientId == query.PatientId, ct);
        var ordered = all.OrderByDescending(c => c.CreatedAt).ToList();
        var total = ordered.Count;
        var items = ordered.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        return Result<PaginatedResult<ConsultDto>>.Success(
            PaginatedResult<ConsultDto>.Create(mapper.Map<IReadOnlyList<ConsultDto>>(items), total, query.PageNumber, query.PageSize));
    }
}
