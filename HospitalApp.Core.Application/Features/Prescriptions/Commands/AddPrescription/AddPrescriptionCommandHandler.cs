using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Prescriptions.Commands.AddPrescription;

public class AddPrescriptionCommandHandler(IUnitOfWork uow)
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
        var prescription = new MedicalPrescription
        {
            ConsultId = command.ConsultId,
            PrescribedByDoctorId = command.DoctorId,
            DrugName = req.DrugName,
            MedicationId = req.MedicationId,
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
