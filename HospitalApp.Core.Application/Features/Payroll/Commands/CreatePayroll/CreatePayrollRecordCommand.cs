using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Payroll.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Payroll.Commands.CreatePayroll;

public record CreatePayrollRecordCommand(
    CreatePayrollRecordRequest Request,
    Guid CreatedByUserId) : IRequest<Result<Guid>>;
