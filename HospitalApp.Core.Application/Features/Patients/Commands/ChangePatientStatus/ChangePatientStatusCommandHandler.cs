using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Commands.ChangePatientStatus;

public class ChangePatientStatusCommandHandler(IUnitOfWork uow)
    : IRequestHandler<ChangePatientStatusCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ChangePatientStatusCommand command, CancellationToken ct)
    {
        var patient = await uow.Patients.GetByIdAsync(command.PatientId, ct);
        if (patient is null)
            return Result<Guid>.NotFound($"Patient {command.PatientId} not found.");

        patient.Status = command.NewStatus;
        patient.UpdatedAt = DateTime.UtcNow;

        uow.Patients.Update(patient);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(patient.Id);
    }
}
