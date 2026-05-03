using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Commands.FinalizeConsult;

public class FinalizeConsultCommandHandler(IUnitOfWork uow)
    : IRequestHandler<FinalizeConsultCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(FinalizeConsultCommand command, CancellationToken ct)
    {
        var consult = await uow.Consults.GetByIdAsync(command.ConsultId, ct);
        if (consult is null)
            return Result<Guid>.NotFound("Consult not found.");

        if (consult.DoctorId != command.DoctorId)
            return Result<Guid>.Forbidden("You can only finalize your own consults.");

        if (consult.Status == ConsultStatusEnum.Finished)
            return Result<Guid>.Failure("Consult already finalized.", 409);

        consult.Status = ConsultStatusEnum.Finished;
        consult.FinishedAt = DateTime.UtcNow;
        consult.UpdatedAt = DateTime.UtcNow;

        // Deduct prescribed medications from inventory
        var prescriptions = await uow.Prescriptions.FindAsync(
            p => p.ConsultId == consult.Id && !p.IsDispensed, ct);

        foreach (var rx in prescriptions)
        {
            var medication = await uow.Medications.FirstOrDefaultAsync(
                m => m.Id.ToString() == rx.MedicationId, ct);

            if (medication is not null && medication.CurrentStock >= rx.QuantityToDispense)
            {
                var stockBefore = medication.CurrentStock;
                medication.CurrentStock -= rx.QuantityToDispense;
                medication.UpdatedAt = DateTime.UtcNow;
                uow.Medications.Update(medication);

                await uow.StockTransactions.AddAsync(new()
                {
                    MedicationId = medication.Id,
                    PerformedByUserId = command.DoctorId,
                    ConsultId = consult.Id,
                    Type = StockTransactionTypeEnum.PrescriptionDispensed,
                    Quantity = -rx.QuantityToDispense,
                    StockBefore = stockBefore,
                    StockAfter = medication.CurrentStock,
                    Reason = $"Dispensed for consult {consult.Id}"
                }, ct);

                rx.IsDispensed = true;
                rx.DispensedAt = DateTime.UtcNow;
                uow.Prescriptions.Update(rx);
            }
        }

        uow.Consults.Update(consult);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(consult.Id);
    }
}
