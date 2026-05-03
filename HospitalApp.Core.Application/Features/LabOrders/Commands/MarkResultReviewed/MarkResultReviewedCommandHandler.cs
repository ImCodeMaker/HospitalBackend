using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Commands.MarkResultReviewed;

public class MarkResultReviewedCommandHandler(IUnitOfWork uow)
    : IRequestHandler<MarkResultReviewedCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(MarkResultReviewedCommand command, CancellationToken ct)
    {
        var order = await uow.LabOrders.GetByIdAsync(command.LabOrderId, ct);
        if (order is null)
            return Result<Guid>.NotFound("Lab order not found.");

        order.ResultReviewedByDoctor = true;
        order.ReviewedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        uow.LabOrders.Update(order);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(order.Id);
    }
}
