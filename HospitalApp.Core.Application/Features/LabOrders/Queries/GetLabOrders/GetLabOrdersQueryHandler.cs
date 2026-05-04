using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.LabOrders.DTOs;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Queries.GetLabOrders;

public class GetLabOrdersQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetLabOrdersQuery, Result<List<LabOrderDto>>>
{
    public async Task<Result<List<LabOrderDto>>> Handle(GetLabOrdersQuery query, CancellationToken ct)
    {
        LabTestPriorityEnum? priorityFilter = null;
        if (!string.IsNullOrEmpty(query.Priority) && Enum.TryParse<LabTestPriorityEnum>(query.Priority, out var parsed))
            priorityFilter = parsed;

        var orders = await uow.LabOrders.FindAsync(o =>
            (string.IsNullOrEmpty(query.Status) || o.Status == query.Status) &&
            (!priorityFilter.HasValue || o.Priority == priorityFilter.Value), ct);

        return Result<List<LabOrderDto>>.Success(
            mapper.Map<List<LabOrderDto>>(orders.OrderByDescending(o => o.CreatedAt).ToList()));
    }
}
