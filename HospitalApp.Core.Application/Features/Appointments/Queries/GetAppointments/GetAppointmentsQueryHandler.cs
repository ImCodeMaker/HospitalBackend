using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Appointments.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler(IUnitOfWork uow, IMapper mapper, IUserContactService userContacts)
    : IRequestHandler<GetAppointmentsQuery, Result<PaginatedResult<AppointmentDto>>>
{
    public async Task<Result<PaginatedResult<AppointmentDto>>> Handle(GetAppointmentsQuery query, CancellationToken ct)
    {
        var all = await uow.Appointments.FindAsync(a =>
            (!query.DoctorId.HasValue || a.AssignedDoctorId == query.DoctorId) &&
            (!query.PatientId.HasValue || a.PatientId == query.PatientId) &&
            (!query.Status.HasValue || a.Status == query.Status) &&
            (!query.From.HasValue || a.ScheduledDate >= query.From) &&
            (!query.To.HasValue || a.ScheduledDate <= query.To), ct);

        var ordered = all.OrderBy(a => a.ScheduledDate).ToList();
        var total = ordered.Count;
        var pageItems = ordered.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        // Resolve patient + doctor names in batches
        var patientIds = pageItems.Select(a => a.PatientId).Distinct().ToList();
        var doctorIds = pageItems.Select(a => a.AssignedDoctorId).Distinct().ToList();

        var patients = await uow.Patients.FindAsync(p => patientIds.Contains(p.Id), ct);
        var patientNames = patients.ToDictionary(p => p.Id, p => $"{p.FirstName} {p.LastName}");

        var doctorNames = new Dictionary<Guid, string>();
        foreach (var did in doctorIds)
        {
            var dc = await userContacts.GetAsync(did, ct);
            doctorNames[did] = dc?.FullName ?? "—";
        }

        var dtos = pageItems.Select(a =>
        {
            var dto = mapper.Map<AppointmentDto>(a);
            return dto with
            {
                PatientName = patientNames.GetValueOrDefault(a.PatientId, "—"),
                DoctorName = doctorNames.GetValueOrDefault(a.AssignedDoctorId, "—"),
            };
        }).ToList();

        return Result<PaginatedResult<AppointmentDto>>.Success(
            PaginatedResult<AppointmentDto>.Create(dtos, total, query.PageNumber, query.PageSize));
    }
}
