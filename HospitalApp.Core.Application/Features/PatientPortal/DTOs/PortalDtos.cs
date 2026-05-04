namespace HospitalApp.Core.Application.Features.PatientPortal.DTOs;

public record PortalProfileDto(
    Guid PatientId,
    string FullName,
    string DocumentType,
    string DocumentNumber,
    DateTime BirthDate,
    int Age,
    string Gender,
    string? Phone,
    string? Email,
    string BloodType,
    string? KnownAllergies,
    string? ChronicConditions,
    bool HasInsurance,
    string? InsuranceCompanyName,
    string? InsurancePolicyNumber,
    decimal InsuranceCoveragePercentage
);

public record PortalAppointmentDto(
    Guid Id,
    DateTime ScheduledDate,
    int DurationMinutes,
    string Type,
    string Status,
    string DoctorName,
    string SpecialtyName,
    string? Reason,
    string? Notes
);

public record PortalConsultSummaryDto(
    Guid Id,
    DateTime CreatedAt,
    string SpecialtyName,
    string DoctorName,
    string Status,
    string? ChiefComplaint,
    string? DiagnosisDescription,
    string? TreatmentPlan,
    DateTime? FinishedAt
);

public record PortalInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    DateTime CreatedAt,
    string Status,
    decimal TotalAmount,
    decimal PatientResponsibilityAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    DateTime? PaidAt
);
