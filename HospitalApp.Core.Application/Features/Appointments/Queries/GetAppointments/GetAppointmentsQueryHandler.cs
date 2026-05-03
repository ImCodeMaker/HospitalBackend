using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Appointments.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler(IUnitOfWork uow, IMapper mapper)
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
        var items = ordered.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        return Result<PaginatedResult<AppointmentDto>>.Success(
            PaginatedResult<AppointmentDto>.Create(
                mapper.Map<IReadOnlyList<AppointmentDto>>(items), total, query.PageNumber, query.PageSize));
    }
}
