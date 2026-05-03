using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreatePatientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePatientCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var duplicate = await uow.Patients.FirstOrDefaultAsync(
            p => p.DocumentType == req.DocumentType && p.DocumentNumber == req.DocumentNumber, ct);

        if (duplicate is not null)
            return Result<Guid>.Failure($"A patient with document {req.DocumentType} {req.DocumentNumber} already exists.", 409);

        var patient = new Patient
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            DocumentType = req.DocumentType,
            DocumentNumber = req.DocumentNumber,
            Nationality = req.Nationality,
            HomeAddress = req.HomeAddress,
            BirthDate = req.BirthDate,
            Gender = req.Gender,
            Status = PatientsStatus.Active,
            Email = req.Email,
            Phone = req.Phone,
            BloodType = req.BloodType,
            KnownAllergies = req.KnownAllergies,
            ChronicConditions = req.ChronicConditions,
            GuardianFirstName = req.GuardianFirstName,
            GuardianLastName = req.GuardianLastName,
            GuardianDocumentType = req.GuardianDocumentType,
            GuardianDocumentNumber = req.GuardianDocumentNumber,
            GuardianRelationship = req.GuardianRelationship,
            GuardianPhone = req.GuardianPhone,
            GuardianEmail = req.GuardianEmail,
            HasInsurance = req.HasInsurance,
            InsuranceCompanyId = req.InsuranceCompanyId,
            InsurancePolicyNumber = req.InsurancePolicyNumber,
            InsurancePolicyHolderName = req.InsurancePolicyHolderName,
            InsuranceCoveragePercentage = req.InsuranceCoveragePercentage,
        };

        await uow.Patients.AddAsync(patient, ct);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Created(patient.Id);
    }
}
