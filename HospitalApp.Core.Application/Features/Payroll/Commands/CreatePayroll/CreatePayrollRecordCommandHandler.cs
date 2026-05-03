using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Payroll.Commands.CreatePayroll;

public class CreatePayrollRecordCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreatePayrollRecordCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePayrollRecordCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var employeeExists = await uow.Employees.ExistsAsync(e => e.Id == req.EmployeeId, ct);
        if (!employeeExists)
            return Result<Guid>.NotFound($"Employee with id '{req.EmployeeId}' not found.");

        var netPay = req.GrossPay - req.AfpEmployee - req.ArsEmployee - req.IsrWithholding;

        var record = new PayrollRecord
        {
            EmployeeId = req.EmployeeId,
            PeriodStart = req.PayPeriodStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            PeriodEnd = req.PayPeriodEnd.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            BaseSalary = req.GrossPay,
            AfpDeduction = req.AfpEmployee,
            ArsDeduction = req.ArsEmployee,
            IsrDeduction = req.IsrWithholding,
            Bonuses = req.AfpEmployer + req.ArsEmployer, // employer contributions stored as bonuses
            NetPay = netPay,
            Status = PayrollStatusEnum.PendingApproval
        };

        await uow.PayrollRecords.AddAsync(record, ct);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Created(record.Id);
    }
}
