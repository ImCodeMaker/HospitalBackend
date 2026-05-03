using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.LabOrders.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Queries.GetLabOrdersByConsult;

public class GetLabOrdersByConsultQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetLabOrdersByConsultQuery, Result<List<LabOrderDto>>>
{
    public async Task<Result<List<LabOrderDto>>> Handle(GetLabOrdersByConsultQuery query, CancellationToken ct)
    {
        var orders = await uow.LabOrders.FindAsync(o => o.ConsultId == query.ConsultId, ct);
        return Result<List<LabOrderDto>>.Success(mapper.Map<List<LabOrderDto>>(orders.OrderBy(o => o.CreatedAt).ToList()));
    }
}
