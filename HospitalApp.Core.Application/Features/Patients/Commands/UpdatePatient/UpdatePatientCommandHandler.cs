using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdatePatientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdatePatientCommand command, CancellationToken ct)
    {
        var patient = await uow.Patients.GetByIdAsync(command.PatientId, ct);
        if (patient is null)
            return Result<Guid>.NotFound($"Patient {command.PatientId} not found.");

        var req = command.Request;
        patient.FirstName = req.FirstName;
        patient.LastName = req.LastName;
        patient.HomeAddress = req.HomeAddress;
        patient.Email = req.Email;
        patient.Phone = req.Phone;
        patient.BloodType = req.BloodType;
        patient.KnownAllergies = req.KnownAllergies;
        patient.ChronicConditions = req.ChronicConditions;
        patient.GuardianFirstName = req.GuardianFirstName;
        patient.GuardianLastName = req.GuardianLastName;
        patient.GuardianDocumentType = req.GuardianDocumentType;
        patient.GuardianDocumentNumber = req.GuardianDocumentNumber;
        patient.GuardianRelationship = req.GuardianRelationship;
        patient.GuardianPhone = req.GuardianPhone;
        patient.GuardianEmail = req.GuardianEmail;
        patient.HasInsurance = req.HasInsurance;
        patient.InsuranceCompanyId = req.InsuranceCompanyId;
        patient.InsurancePolicyNumber = req.InsurancePolicyNumber;
        patient.InsurancePolicyHolderName = req.InsurancePolicyHolderName;
        patient.InsuranceCoveragePercentage = req.InsuranceCoveragePercentage;
        patient.UpdatedAt = DateTime.UtcNow;

        uow.Patients.Update(patient);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(patient.Id);
    }
}
