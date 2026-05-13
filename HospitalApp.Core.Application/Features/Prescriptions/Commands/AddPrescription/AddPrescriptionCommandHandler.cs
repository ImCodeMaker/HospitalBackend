using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Prescriptions.Commands.AddPrescription;

public class AddPrescriptionCommandHandler(IUnitOfWork uow, IDrugInteractionService interactions)
    : IRequestHandler<AddPrescriptionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddPrescriptionCommand command, CancellationToken ct)
    {
        var consult = await uow.Consults.GetByIdAsync(command.ConsultId, ct);
        if (consult is null)
            return Result<Guid>.NotFound("Consult not found.");

        if (consult.Status == ConsultStatusEnum.Finished)
            return Result<Guid>.Failure("Cannot add prescriptions to a finished consult.", 409);

        var req = command.Request;

        // Auto-resolve RxCUI from drug name if not supplied
        var rxCui = req.RxCui;
        if (string.IsNullOrEmpty(rxCui) && !string.IsNullOrEmpty(req.DrugName))
        {
            rxCui = await interactions.ResolveRxCuiAsync(req.DrugName, ct);
        }

        if (!string.IsNullOrEmpty(rxCui) && !req.AcknowledgeInteractions)
        {
            var existingRxCuis = (await uow.Prescriptions
                .FindAsync(p => p.ConsultId == command.ConsultId, ct))
                .Select(p => p.RxCui)
                .Where(c => !string.IsNullOrEmpty(c))
                .Select(c => c!)
                .ToList();

            // Also pull RxCuis from prior consults of the same patient (active therapy)
            var activeRxCuisFromOtherConsults = (await uow.Prescriptions
                .FindAsync(p => p.Consult != null
                                && p.Consult.PatientId == consult.PatientId
                                && p.ConsultId != command.ConsultId
                                && !p.IsDispensed, ct))
                .Select(p => p.RxCui)
                .Where(c => !string.IsNullOrEmpty(c))
                .Select(c => c!)
                .ToList();

            var allRxCuis = existingRxCuis
                .Concat(activeRxCuisFromOtherConsults)
                .Append(rxCui)
                .Distinct()
                .ToList();

            if (allRxCuis.Count >= 2)
            {
                var alerts = await interactions.CheckInteractionsAsync(allRxCuis, ct);
                var critical = alerts.Where(a => string.Equals(a.Severity, "high", StringComparison.OrdinalIgnoreCase)
                                              || string.Equals(a.Severity, "major", StringComparison.OrdinalIgnoreCase)
                                              || string.Equals(a.Severity, "severe", StringComparison.OrdinalIgnoreCase))
                                    .ToList();
                if (critical.Count > 0)
                {
                    var msg = "Drug interaction alert: " + string.Join("; ",
                        critical.Select(a => $"{a.Drug1} ↔ {a.Drug2} ({a.Severity}) — {a.Description}"));
                    return Result<Guid>.Failure(msg, 409);
                }
            }
        }

        var prescription = new MedicalPrescription
        {
            ConsultId = command.ConsultId,
            PrescribedByDoctorId = command.DoctorId,
            DrugName = req.DrugName,
            MedicationId = req.MedicationId,
            RxCui = rxCui,
            Presentation = req.Presentation,
            Dosage = req.Dosage,
            Frequency = req.Frequency,
            RouteOfAdministration = req.RouteOfAdministration,
            DurationDays = req.DurationDays,
            QuantityToDispense = req.QuantityToDispense,
            SpecialInstructions = req.SpecialInstructions,
        };

        await uow.Prescriptions.AddAsync(prescription, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Created(prescription.Id);
    }
}
