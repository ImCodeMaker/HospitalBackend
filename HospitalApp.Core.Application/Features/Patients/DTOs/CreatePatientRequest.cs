using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.Patients.DTOs;

public class CreatePatientRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DocumentTypeEnum DocumentType { get; init; }
    public string DocumentNumber { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public string HomeAddress { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public GendersEnum Gender { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public BloodTypeEnum BloodType { get; init; } = BloodTypeEnum.Unknown;
    public string? KnownAllergies { get; init; }
    public string? ChronicConditions { get; init; }

    // Guardian (required when IsMinor based on BirthDate)
    public string? GuardianFirstName { get; init; }
    public string? GuardianLastName { get; init; }
    public DocumentTypeEnum? GuardianDocumentType { get; init; }
    public string? GuardianDocumentNumber { get; init; }
    public GuardianRelationshipEnum? GuardianRelationship { get; init; }
    public string? GuardianPhone { get; init; }
    public string? GuardianEmail { get; init; }

    // Insurance
    public bool HasInsurance { get; init; }
    public Guid? InsuranceCompanyId { get; init; }
    public string? InsurancePolicyNumber { get; init; }
    public string? InsurancePolicyHolderName { get; init; }
    public decimal InsuranceCoveragePercentage { get; init; }
}
