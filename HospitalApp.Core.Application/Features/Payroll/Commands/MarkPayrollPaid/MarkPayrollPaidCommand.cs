using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Payroll.Commands.MarkPayrollPaid;

public record MarkPayrollPaidCommand(
    Guid PayrollId,
    Guid MarkedByUserId,
    DateTime PaymentDate,
    string? PaymentReference) : IRequest<Result>;
