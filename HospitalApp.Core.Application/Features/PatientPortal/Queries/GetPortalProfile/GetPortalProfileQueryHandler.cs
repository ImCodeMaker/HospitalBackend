using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.PatientPortal.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalProfile;

public class GetPortalProfileQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetPortalProfileQuery, Result<PortalProfileDto>>
{
    public async Task<Result<PortalProfileDto>> Handle(GetPortalProfileQuery query, CancellationToken ct)
    {
        var patient = await uow.Patients.GetByIdAsync(query.PatientId, ct);
        if (patient is null)
            return Result<PortalProfileDto>.NotFound("Patient record not found.");

        var insuranceName = patient.InsuranceCompany?.Name;
        if (insuranceName is null && patient.InsuranceCompanyId.HasValue)
        {
            var company = await uow.InsuranceCompanies.GetByIdAsync(patient.InsuranceCompanyId.Value, ct);
            insuranceName = company?.Name;
        }

        return Result<PortalProfileDto>.Success(new PortalProfileDto(
            patient.Id,
            $"{patient.FirstName} {patient.LastName}",
            patient.DocumentType.ToString(),
            patient.DocumentNumber,
            patient.BirthDate,
            DateTime.UtcNow.Year - patient.BirthDate.Year,
            patient.Gender.ToString(),
            patient.Phone,
            patient.Email,
            patient.BloodType.ToString(),
            patient.KnownAllergies,
            patient.ChronicConditions,
            patient.HasInsurance,
            insuranceName,
            patient.InsurancePolicyNumber,
            patient.InsuranceCoveragePercentage
        ));
    }
}
