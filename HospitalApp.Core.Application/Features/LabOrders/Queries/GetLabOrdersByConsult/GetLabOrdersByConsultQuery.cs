using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.LabOrders.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Queries.GetLabOrdersByConsult;

public record GetLabOrdersByConsultQuery(Guid ConsultId) : IRequest<Result<List<LabOrderDto>>>;
