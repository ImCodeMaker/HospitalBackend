using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Caja.Commands.OpenShift;

public class OpenShiftCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier)
    : IRequestHandler<OpenShiftCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(OpenShiftCommand command, CancellationToken ct)
    {
        var openShift = await uow.CajaShifts.FirstOrDefaultAsync(s => s.IsOpen, ct);
        if (openShift is not null)
            return Result<Guid>.Failure("A shift is already open. Close it before opening a new one.", 409);

        var shift = new CajaShift
        {
            OpenedByUserId = command.UserId,
            OpeningBalance = command.OpeningBalance,
        };

        await uow.CajaShifts.AddAsync(shift, ct);
        await uow.SaveChangesAsync(ct);
        await notifier.NotifyCajaChangedAsync(ct);
        return Result<Guid>.Created(shift.Id);
    }
}
