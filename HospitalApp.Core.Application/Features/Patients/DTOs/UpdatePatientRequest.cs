using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.Patients.DTOs;

public class UpdatePatientRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string HomeAddress { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public BloodTypeEnum BloodType { get; init; }
    public string? KnownAllergies { get; init; }
    public string? ChronicConditions { get; init; }

    public string? GuardianFirstName { get; init; }
    public string? GuardianLastName { get; init; }
    public DocumentTypeEnum? GuardianDocumentType { get; init; }
    public string? GuardianDocumentNumber { get; init; }
    public GuardianRelationshipEnum? GuardianRelationship { get; init; }
    public string? GuardianPhone { get; init; }
    public string? GuardianEmail { get; init; }

    public bool HasInsurance { get; init; }
    public Guid? InsuranceCompanyId { get; init; }
    public string? InsurancePolicyNumber { get; init; }
    public string? InsurancePolicyHolderName { get; init; }
    public decimal InsuranceCoveragePercentage { get; init; }
}
