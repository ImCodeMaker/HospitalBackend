using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Caja.Commands.AddCashTransaction;

public class AddCashTransactionCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier)
    : IRequestHandler<AddCashTransactionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddCashTransactionCommand command, CancellationToken ct)
    {
        var shift = await uow.CajaShifts.GetByIdAsync(command.ShiftId, ct);
        if (shift is null)
            return Result<Guid>.NotFound("Shift not found.");
        if (!shift.IsOpen)
            return Result<Guid>.Failure("Cannot add transactions to a closed shift.", 409);

        var requiresApproval = command.Type == CashTransactionTypeEnum.CashAdvance;
        var tx = new CashTransaction
        {
            ShiftId = command.ShiftId,
            Type = command.Type,
            Amount = command.Amount,
            Description = command.Description,
            InvoiceId = command.InvoiceId,
            CreatedByUserId = command.CreatedByUserId,
            RequiresAdminApproval = requiresApproval,
            IsApproved = !requiresApproval,
        };

        await uow.CashTransactions.AddAsync(tx, ct);
        await uow.SaveChangesAsync(ct);
        await notifier.NotifyCajaChangedAsync(ct);
        return Result<Guid>.Created(tx.Id);
    }
}
