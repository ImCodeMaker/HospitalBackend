using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Payroll.Commands.ApprovePayroll;

public class ApprovePayrollCommandHandler(IUnitOfWork uow)
    : IRequestHandler<ApprovePayrollCommand, Result>
{
    public async Task<Result> Handle(ApprovePayrollCommand command, CancellationToken ct)
    {
        var record = await uow.PayrollRecords.GetByIdAsync(command.PayrollId, ct);
        if (record is null)
            return Result.NotFound("Payroll record not found.");

        if (record.Status != PayrollStatusEnum.PendingApproval)
            return Result.Failure("Only payroll records with status 'PendingApproval' can be approved.", 422);

        record.Status = PayrollStatusEnum.Approved;
        record.ApprovedByUserId = command.ApprovedByUserId;
        record.UpdatedAt = DateTime.UtcNow;

        uow.PayrollRecords.Update(record);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
