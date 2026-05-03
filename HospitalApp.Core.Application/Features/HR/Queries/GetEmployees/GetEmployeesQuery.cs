using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.HR.DTOs;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.HR.Queries.GetEmployees;

public record GetEmployeesQuery(
    EmployeeRoleEnum? Role,
    EmployeeStatusEnum? Status,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedResult<EmployeeDto>>>;
