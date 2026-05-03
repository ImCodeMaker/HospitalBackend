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
    string? SpecialInstructions
);

public record AddPrescriptionCommand(Guid ConsultId, AddPrescriptionRequest Request, Guid DoctorId)
    : IRequest<Result<Guid>>;
