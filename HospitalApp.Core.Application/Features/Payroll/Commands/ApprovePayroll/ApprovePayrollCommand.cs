using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Payroll.Commands.ApprovePayroll;

public record ApprovePayrollCommand(Guid PayrollId, Guid ApprovedByUserId) : IRequest<Result>;
