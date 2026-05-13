using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Prescriptions.Commands.AddPrescription;

public record AddPrescriptionRequest(
    string DrugName,
    string? MedicationId,
    string Presentation,
    string Dosage,
    string Frequency,
    string RouteOfAdministration,
    int DurationDays,
    int QuantityToDispense,
    string? SpecialInstructions,
    /// <summary>RxCUI for the drug being prescribed; enables interaction check.</summary>
    string? RxCui = null,
    /// <summary>Doctor overrides interaction warnings (clinical judgment).</summary>
    bool AcknowledgeInteractions = false
);

public record AddPrescriptionCommand(Guid ConsultId, AddPrescriptionRequest Request, Guid DoctorId)
    : IRequest<Result<Guid>>;
