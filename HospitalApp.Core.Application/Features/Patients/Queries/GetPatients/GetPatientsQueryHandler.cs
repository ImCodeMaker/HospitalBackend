using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Patients.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Queries.GetPatients;

public class GetPatientsQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetPatientsQuery, Result<PaginatedResult<PatientDto>>>
{
    public async Task<Result<PaginatedResult<PatientDto>>> Handle(GetPatientsQuery query, CancellationToken ct)
    {
        var allPatients = await uow.Patients.FindAsync(p =>
            (query.Status == null || p.Status == query.Status) &&
            (string.IsNullOrEmpty(query.SearchTerm) ||
             p.FirstName.Contains(query.SearchTerm) ||
             p.LastName.Contains(query.SearchTerm) ||
             p.DocumentNumber.Contains(query.SearchTerm)), ct);

        var total = allPatients.Count;
        var items = allPatients
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var dtos = mapper.Map<IReadOnlyList<PatientDto>>(items);
        return Result<PaginatedResult<PatientDto>>.Success(
            PaginatedResult<PatientDto>.Create(dtos, total, query.PageNumber, query.PageSize));
    }
}
