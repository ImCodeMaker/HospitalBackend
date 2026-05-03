using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.HR.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.HR.Queries.GetEmployeePerformance;

public record GetEmployeePerformanceQuery(Guid EmployeeId, DateTime? From, DateTime? To)
    : IRequest<Result<EmployeePerformanceDto>>;
