using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Caja.Commands.CloseShift;

public class CloseShiftCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier, IEmailService email)
    : IRequestHandler<CloseShiftCommand, Result<Guid>>
{
    private const decimal DiscrepancyAlertThreshold = 200m; // RD$200
    public async Task<Result<Guid>> Handle(CloseShiftCommand command, CancellationToken ct)
    {
        var shift = await uow.CajaShifts.GetByIdAsync(command.ShiftId, ct);
        if (shift is null)
            return Result<Guid>.NotFound("Shift not found.");
        if (!shift.IsOpen)
            return Result<Guid>.Failure("Shift is already closed.", 409);

        var transactions = await uow.CashTransactions.FindAsync(t => t.ShiftId == command.ShiftId, ct);
        var expectedBalance = shift.OpeningBalance
            + transactions.Where(t => t.IsApproved).Sum(t => t.Type switch
            {
                Domain.Enums.CashTransactionTypeEnum.PaymentCash => t.Amount,
                Domain.Enums.CashTransactionTypeEnum.PaymentCard => t.Amount,
                Domain.Enums.CashTransactionTypeEnum.BankTransfer => t.Amount,
                Domain.Enums.CashTransactionTypeEnum.CashRefund => -t.Amount,
                Domain.Enums.CashTransactionTypeEnum.CashAdvance => -t.Amount,
                Domain.Enums.CashTransactionTypeEnum.PettyCashExpense => -t.Amount,
                _ => 0m
            });

        shift.ClosingBalance = command.ClosingBalance;
        shift.ExpectedBalance = expectedBalance;
        shift.Discrepancy = command.ClosingBalance - expectedBalance;
        shift.ClosedAt = DateTime.UtcNow;
        shift.ClosedByUserId = command.UserId;
        shift.IsOpen = false;
        shift.Notes = command.Notes;

        uow.CajaShifts.Update(shift);
        await uow.SaveChangesAsync(ct);
        await notifier.NotifyCajaChangedAsync(ct);

        var discrepancy = shift.Discrepancy ?? 0m;
        if (Math.Abs(discrepancy) >= DiscrepancyAlertThreshold)
        {
            var settings = await uow.ClinicSettings.FirstOrDefaultAsync(_ => true, ct);
            var adminEmail = settings?.Email;
            if (!string.IsNullOrEmpty(adminEmail))
            {
                var sign = discrepancy > 0 ? "+" : "";
                await email.SendAsync(adminEmail,
                    $"⚠ Caja discrepancy alert — RD${sign}{discrepancy:N2}",
                    $"""
                    <h2>Caja Shift Discrepancy Detected</h2>
                    <p>Shift closed by user <strong>{command.UserId}</strong> at <strong>{shift.ClosedAt:yyyy-MM-dd HH:mm} UTC</strong>.</p>
                    <table>
                      <tr><td>Expected balance</td><td><strong>RD$ {expectedBalance:N2}</strong></td></tr>
                      <tr><td>Actual (counted)</td><td><strong>RD$ {command.ClosingBalance:N2}</strong></td></tr>
                      <tr><td>Discrepancy</td><td><strong style="color:red">RD$ {sign}{discrepancy:N2}</strong></td></tr>
                    </table>
                    <p>{(shift.Notes is not null ? $"Notes: {shift.Notes}" : "No notes provided.")}</p>
                    """, ct);
            }
        }

        return Result<Guid>.Success(shift.Id);
    }
}
