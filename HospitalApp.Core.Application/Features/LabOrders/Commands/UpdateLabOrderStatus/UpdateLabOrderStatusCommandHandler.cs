using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Commands.UpdateLabOrderStatus;

public class UpdateLabOrderStatusCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateLabOrderStatusCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateLabOrderStatusCommand command, CancellationToken ct)
    {
        var order = await uow.LabOrders.GetByIdAsync(command.LabOrderId, ct);
        if (order is null) return Result<Guid>.NotFound("Lab order not found.");

        order.Status = command.Status;
        if (command.Status == "InProgress") order.SampleCollectedAt ??= DateTime.UtcNow;
        if (command.Status == "Completed") order.ResultsAvailableAt ??= DateTime.UtcNow;

        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(order.Id);
    }
}
