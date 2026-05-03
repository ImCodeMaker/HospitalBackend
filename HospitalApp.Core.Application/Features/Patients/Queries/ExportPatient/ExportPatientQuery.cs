using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Queries.ExportPatient;

public record ExportPatientQuery(Guid PatientId) : IRequest<Result<PatientExportDto>>;

public record PatientExportDto(
    // Identity
    Guid Id,
    string FirstName,
    string LastName,
    string DocumentType,
    string DocumentNumber,
    string Nationality,
    string HomeAddress,
    DateTime BirthDate,
    string Gender,
    string Status,

    // Contact
    string? Email,
    string? Phone,

    // Clinical
    string BloodType,
    string? KnownAllergies,
    string? ChronicConditions,

    // Guardian
    bool IsMinor,
    string? GuardianFirstName,
    string? GuardianLastName,
    string? GuardianDocumentType,
    string? GuardianDocumentNumber,
    string? GuardianRelationship,
    string? GuardianPhone,
    string? GuardianEmail,

    // Insurance
    bool HasInsurance,
    string? InsurancePolicyNumber,
    string? InsurancePolicyHolderName,
    decimal InsuranceCoveragePercentage,

    // Timestamps
    DateTime CreatedAt,
    DateTime? UpdatedAt,

    // Collections
    List<ConsultSummary> Consults,
    List<InvoiceSummary> Invoices,
    List<AppointmentSummary> Appointments
);

public record ConsultSummary(
    Guid ConsultId,
    DateTime Date,
    string Status,
    string? DiagnosisDescription
);

public record InvoiceSummary(
    Guid InvoiceId,
    DateTime Date,
    decimal TotalAmount,
    string Status
);

public record AppointmentSummary(
    Guid AppointmentId,
    DateTime Date,
    string Type,
    string Status
);
