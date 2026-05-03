using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.HR.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.HR.Queries.GetEmployees;

public class GetEmployeesQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetEmployeesQuery, Result<PaginatedResult<EmployeeDto>>>
{
    public async Task<Result<PaginatedResult<EmployeeDto>>> Handle(GetEmployeesQuery query, CancellationToken ct)
    {
        var employees = await uow.Employees.FindAsync(e =>
            (query.Role == null || e.Role == query.Role) &&
            (query.Status == null || e.Status == query.Status), ct);

        var total = employees.Count;
        var paged = employees
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return Result<PaginatedResult<EmployeeDto>>.Success(
            PaginatedResult<EmployeeDto>.Create(mapper.Map<List<EmployeeDto>>(paged), total, query.PageNumber, query.PageSize));
    }
}
