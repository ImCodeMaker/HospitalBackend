using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.Patients.DTOs;

public class PatientDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string DocumentType { get; init; } = string.Empty;
    public string DocumentNumber { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public string HomeAddress { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public int Age => DateTime.UtcNow.Year - BirthDate.Year;
    public bool IsMinor => Age < 18;
    public string Gender { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string BloodType { get; init; } = string.Empty;
    public string? KnownAllergies { get; init; }
    public string? ChronicConditions { get; init; }

    // Guardian
    public string? GuardianFirstName { get; init; }
    public string? GuardianLastName { get; init; }
    public string? GuardianRelationship { get; init; }
    public string? GuardianPhone { get; init; }
    public string? GuardianEmail { get; init; }

    // Insurance
    public bool HasInsurance { get; init; }
    public string? InsuranceCompanyName { get; init; }
    public string? InsurancePolicyNumber { get; init; }
    public decimal InsuranceCoveragePercentage { get; init; }

    public DateTime CreatedAt { get; init; }
}
