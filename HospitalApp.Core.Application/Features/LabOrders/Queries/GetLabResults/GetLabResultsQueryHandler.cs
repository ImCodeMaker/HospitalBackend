using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.LabOrders.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Queries.GetLabResults;

public class GetLabResultsQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetLabResultsQuery, Result<List<LabResultDto>>>
{
    public async Task<Result<List<LabResultDto>>> Handle(GetLabResultsQuery query, CancellationToken ct)
    {
        var results = await uow.LabResults.FindAsync(r => r.LabOrderId == query.LabOrderId, ct);
        return Result<List<LabResultDto>>.Success(
            mapper.Map<List<LabResultDto>>(results.OrderBy(r => r.CreatedAt).ToList()));
    }
}
