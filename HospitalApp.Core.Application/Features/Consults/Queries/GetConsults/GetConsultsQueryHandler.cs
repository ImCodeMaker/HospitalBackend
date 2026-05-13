using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Queries.GetConsults;

public class GetConsultsQueryHandler(IUnitOfWork uow, IMapper mapper, IUserContactService userContacts)
    : IRequestHandler<GetConsultsQuery, Result<PaginatedResult<ConsultDto>>>
{
    public async Task<Result<PaginatedResult<ConsultDto>>> Handle(GetConsultsQuery query, CancellationToken ct)
    {
        ConsultStatusEnum? statusFilter = null;
        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<ConsultStatusEnum>(query.Status, out var parsed))
            statusFilter = parsed;

        var all = await uow.Consults.FindAsync(
            c => !statusFilter.HasValue || c.Status == statusFilter.Value, ct);

        var ordered = all.OrderByDescending(c => c.CreatedAt).ToList();
        var total = ordered.Count;
        var items = ordered.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        // Batch-resolve names
        var patientIds = items.Select(c => c.PatientId).Distinct().ToList();
        var specialtyIds = items.Select(c => c.SpecialtyId).Distinct().ToList();
        var doctorIds = items.Select(c => c.DoctorId).Distinct().ToList();

        var patients = await uow.Patients.FindAsync(p => patientIds.Contains(p.Id), ct);
        var patientNames = patients.ToDictionary(p => p.Id, p => $"{p.FirstName} {p.LastName}");

        var specialties = await uow.Specialties.FindAsync(s => specialtyIds.Contains(s.Id), ct);
        var specialtyNames = specialties.ToDictionary(s => s.Id, s => s.Name);

        var doctorNames = new Dictionary<Guid, string>();
        foreach (var did in doctorIds)
        {
            var dc = await userContacts.GetAsync(did, ct);
            doctorNames[did] = dc?.FullName ?? "—";
        }

        var dtos = items.Select(c =>
        {
            var dto = mapper.Map<ConsultDto>(c);
            return dto with
            {
                PatientName = patientNames.GetValueOrDefault(c.PatientId, "—"),
                SpecialtyName = specialtyNames.GetValueOrDefault(c.SpecialtyId, "—"),
                DoctorName = doctorNames.GetValueOrDefault(c.DoctorId, "—"),
            };
        }).ToList();

        return Result<PaginatedResult<ConsultDto>>.Success(
            PaginatedResult<ConsultDto>.Create(dtos, total, query.PageNumber, query.PageSize));
    }
}
