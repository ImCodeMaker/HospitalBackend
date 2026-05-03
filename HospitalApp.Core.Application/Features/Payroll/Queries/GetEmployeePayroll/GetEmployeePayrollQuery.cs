using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Payroll.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Payroll.Queries.GetEmployeePayroll;

public record GetEmployeePayrollQuery(Guid EmployeeId, int Page = 1, int PageSize = 20)
    : IRequest<Result<PaginatedResult<PayrollRecordDto>>>;
