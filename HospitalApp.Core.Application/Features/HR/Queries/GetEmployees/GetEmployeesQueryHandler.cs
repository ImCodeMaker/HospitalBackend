using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.HR.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.HR.Queries.GetEmployees;

public class GetEmployeesQueryHandler(IUnitOfWork uow, IMapper mapper, IUserContactService userContacts)
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

        var specialtyIds = paged.Where(e => e.SpecialtyId.HasValue).Select(e => e.SpecialtyId!.Value).Distinct().ToList();
        var specialties = await uow.Specialties.FindAsync(s => specialtyIds.Contains(s.Id), ct);
        var specialtyMap = specialties.ToDictionary(s => s.Id, s => s.Name);

        var dtos = new List<EmployeeDto>(paged.Count);
        foreach (var e in paged)
        {
            var dto = mapper.Map<EmployeeDto>(e);
            string? email = null, phone = null;
            if (e.UserId.HasValue)
            {
                var contact = await userContacts.GetAsync(e.UserId.Value, ct);
                email = contact?.Email;
                phone = contact?.Phone;
            }
            dtos.Add(dto with
            {
                Email = email,
                Phone = phone,
                SpecialtyName = e.SpecialtyId.HasValue ? specialtyMap.GetValueOrDefault(e.SpecialtyId.Value) : null,
            });
        }

        return Result<PaginatedResult<EmployeeDto>>.Success(
            PaginatedResult<EmployeeDto>.Create(dtos, total, query.PageNumber, query.PageSize));
    }
}
