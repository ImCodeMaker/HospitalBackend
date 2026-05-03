using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Commands.AnonymizePatient;

public class AnonymizePatientCommandHandler(IUnitOfWork uow)
    : IRequestHandler<AnonymizePatientCommand, Result>
{
    public async Task<Result> Handle(AnonymizePatientCommand command, CancellationToken ct)
    {
        var patient = await uow.Patients.GetByIdAsync(command.PatientId, ct);
        if (patient is null)
            return Result.NotFound($"Patient {command.PatientId} not found.");

        // PII fields — replace with ANONYMIZED
        patient.FirstName      = "ANONYMIZED";
        patient.LastName       = "ANONYMIZED";
        patient.DocumentNumber = "ANONYMIZED";
        patient.HomeAddress    = "ANONYMIZED";

        patient.Phone          = null;
        patient.Email          = null;

        // Guardian PII (only if previously set)
        if (patient.GuardianFirstName is not null)
            patient.GuardianFirstName = "ANONYMIZED";
        if (patient.GuardianLastName is not null)
            patient.GuardianLastName = "ANONYMIZED";
        if (patient.GuardianDocumentNumber is not null)
            patient.GuardianDocumentNumber = "ANONYMIZED";
        if (patient.GuardianPhone is not null)
            patient.GuardianPhone = null;
        if (patient.GuardianEmail is not null)
            patient.GuardianEmail = null;

        // Insurance PII
        if (patient.InsurancePolicyNumber is not null)
            patient.InsurancePolicyNumber = "ANONYMIZED";

        // Status → Archived
        patient.Status    = PatientsStatus.Archived;
        patient.UpdatedAt = DateTime.UtcNow;

        uow.Patients.Update(patient);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
