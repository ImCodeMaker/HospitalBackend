using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Commands.UpdateLabOrderStatus;

public record UpdateLabOrderStatusCommand(Guid LabOrderId, string Status) : IRequest<Result<Guid>>;
