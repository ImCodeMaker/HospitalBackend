using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.LabOrders.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Queries.GetLabResults;

public record GetLabResultsQuery(Guid LabOrderId) : IRequest<Result<List<LabResultDto>>>;
