using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class Patient : SharedEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required DocumentTypeEnum DocumentType { get; set; }
    public required string DocumentNumber { get; set; }
    public required string Nationality { get; set; }
    public required string HomeAddress { get; set; }
    public required DateTime BirthDate { get; set; }
    public required GendersEnum Gender { get; set; }
    public required PatientsStatus Status { get; set; } = PatientsStatus.Active;

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public BloodTypeEnum BloodType { get; set; } = BloodTypeEnum.Unknown;
    public string? KnownAllergies { get; set; }
    public string? ChronicConditions { get; set; }

    // Minor-only guardian fields
    public bool IsMinor => DateTime.UtcNow.Year - BirthDate.Year < 18;
    public string? GuardianFirstName { get; set; }
    public string? GuardianLastName { get; set; }
    public DocumentTypeEnum? GuardianDocumentType { get; set; }
    public string? GuardianDocumentNumber { get; set; }
    public GuardianRelationshipEnum? GuardianRelationship { get; set; }
    public string? GuardianPhone { get; set; }
    public string? GuardianEmail { get; set; }

    // Insurance
    public bool HasInsurance { get; set; }
    public Guid? InsuranceCompanyId { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsurancePolicyHolderName { get; set; }
    public decimal InsuranceCoveragePercentage { get; set; }
    public string? InsuranceCardImagePath { get; set; }

    // Navigation
    public InsuranceCompany? InsuranceCompany { get; set; }
    public ICollection<Consult> Consults { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
}
