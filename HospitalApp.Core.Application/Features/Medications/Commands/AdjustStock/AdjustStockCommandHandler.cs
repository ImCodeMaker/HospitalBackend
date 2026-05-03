using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Medications.Commands.AdjustStock;

public class AdjustStockCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier)
    : IRequestHandler<AdjustStockCommand, Result<int>>
{
    public async Task<Result<int>> Handle(AdjustStockCommand command, CancellationToken ct)
    {
        var medication = await uow.Medications.GetByIdAsync(command.MedicationId, ct);
        if (medication is null)
            return Result<int>.NotFound("Medication not found.");

        var stockBefore = medication.CurrentStock;
        var newStock = stockBefore + command.Quantity;

        if (newStock < 0)
            return Result<int>.Failure($"Insufficient stock. Current: {stockBefore}, requested: {Math.Abs(command.Quantity)}.");

        medication.CurrentStock = newStock;
        medication.UpdatedAt = DateTime.UtcNow;
        uow.Medications.Update(medication);

        await uow.StockTransactions.AddAsync(new StockTransaction
        {
            MedicationId = medication.Id,
            PerformedByUserId = command.PerformedByUserId,
            Type = command.Type,
            Quantity = command.Quantity,
            StockBefore = stockBefore,
            StockAfter = newStock,
            Reason = command.Reason
        }, ct);

        await uow.SaveChangesAsync(ct);
        await notifier.NotifyInventoryChangedAsync(ct);
        return Result<int>.Success(newStock);
    }
}
