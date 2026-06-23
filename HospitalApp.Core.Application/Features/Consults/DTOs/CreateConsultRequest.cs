using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.Consults.DTOs;

public class CreateConsultRequest
{
    /// <summary>Existing patient ID. Omit when QuickPatient is supplied for inline registration.</summary>
    public Guid? PatientId { get; init; }
    public Guid SpecialtyId { get; init; }

    /// <summary>Quick-register a new patient as PendingVerification when PatientId is null.</summary>
    public QuickPatientRequest? QuickPatient { get; init; }

    // Vitals (optional at creation)
    public decimal? WeightKg { get; init; }
    public decimal? HeightCm { get; init; }
    public int? BpSystolic { get; init; }
    public int? BpDiastolic { get; init; }
    public int? HeartRate { get; init; }
    public decimal? TemperatureCelsius { get; init; }
    public decimal? O2Saturation { get; init; }
    public int? RespiratoryRate { get; init; }

    public string? ChiefComplaint { get; init; }

    /// <summary>Required upfront payment collected before the consult is opened.</summary>
    public required PrepaidConsultPaymentRequest Payment { get; init; }
}

public class PrepaidConsultPaymentRequest
{
    public decimal Amount { get; init; }
    public PaymentMethodEnum Method { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Notes { get; init; }
    public NcfTypeEnum? NcfType { get; init; }
}

public class QuickPatientRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public DocumentTypeEnum DocumentType { get; init; } = DocumentTypeEnum.Cedula;
    public required string DocumentNumber { get; init; }
    public required DateTime BirthDate { get; init; }
    public GendersEnum Gender { get; init; } = GendersEnum.Other;
    public string? Nationality { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}
