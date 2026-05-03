using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Appointments.Commands.UpdateAppointmentStatus;

public class UpdateAppointmentStatusCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier)
    : IRequestHandler<UpdateAppointmentStatusCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateAppointmentStatusCommand command, CancellationToken ct)
    {
        var apt = await uow.Appointments.GetByIdAsync(command.AppointmentId, ct);
        if (apt is null)
            return Result<Guid>.NotFound("Appointment not found.");

        apt.Status = command.NewStatus;
        if (command.Notes is not null) apt.Notes = command.Notes;
        apt.UpdatedAt = DateTime.UtcNow;

        uow.Appointments.Update(apt);
        await uow.SaveChangesAsync(ct);
        await notifier.NotifyAppointmentChangedAsync(ct);

        return Result<Guid>.Success(apt.Id);
    }
}
