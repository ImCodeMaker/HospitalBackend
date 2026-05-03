using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Payroll.Commands.MarkPayrollPaid;

public class MarkPayrollPaidCommandHandler(IUnitOfWork uow)
    : IRequestHandler<MarkPayrollPaidCommand, Result>
{
    public async Task<Result> Handle(MarkPayrollPaidCommand command, CancellationToken ct)
    {
        var record = await uow.PayrollRecords.GetByIdAsync(command.PayrollId, ct);
        if (record is null)
            return Result.NotFound("Payroll record not found.");

        if (record.Status != PayrollStatusEnum.Approved)
            return Result.Failure("Only payroll records with status 'Approved' can be marked as paid.", 409);

        record.Status = PayrollStatusEnum.Paid;
        record.PaymentDate = command.PaymentDate;
        record.ReferenceNumber = command.PaymentReference;
        record.UpdatedAt = DateTime.UtcNow;

        uow.PayrollRecords.Update(record);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
