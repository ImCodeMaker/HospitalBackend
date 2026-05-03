using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Payroll.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Payroll.Queries.GetEmployeePayroll;

public class GetEmployeePayrollQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetEmployeePayrollQuery, Result<PaginatedResult<PayrollRecordDto>>>
{
    public async Task<Result<PaginatedResult<PayrollRecordDto>>> Handle(GetEmployeePayrollQuery query, CancellationToken ct)
    {
        var all = await uow.PayrollRecords.FindAsync(r => r.EmployeeId == query.EmployeeId, ct);

        var ordered = all.OrderByDescending(r => r.PeriodStart).ToList();
        var paged = ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var dtos = paged.Select(r => new PayrollRecordDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = null,
            PayPeriodStart = DateOnly.FromDateTime(r.PeriodStart),
            PayPeriodEnd = DateOnly.FromDateTime(r.PeriodEnd),
            GrossPay = r.BaseSalary,
            AfpEmployee = r.AfpDeduction,
            AfpEmployer = 0m,
            ArsEmployee = r.ArsDeduction,
            ArsEmployer = 0m,
            IsrWithholding = r.IsrDeduction,
            NetPay = r.NetPay,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt
        }).ToList();

        return Result<PaginatedResult<PayrollRecordDto>>.Success(
            PaginatedResult<PayrollRecordDto>.Create(dtos, ordered.Count, query.Page, query.PageSize));
    }
}
