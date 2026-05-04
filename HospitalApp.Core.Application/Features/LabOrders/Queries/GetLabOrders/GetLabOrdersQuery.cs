using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.LabOrders.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Queries.GetLabOrders;

public record GetLabOrdersQuery(
    string? Status = null,
    string? Priority = null
) : IRequest<Result<List<LabOrderDto>>>;
