using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Commands.MarkResultReviewed;

public record MarkResultReviewedCommand(Guid LabOrderId, Guid DoctorId) : IRequest<Result<Guid>>;
