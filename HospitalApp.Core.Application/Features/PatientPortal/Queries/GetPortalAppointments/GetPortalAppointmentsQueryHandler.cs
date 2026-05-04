using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.PatientPortal.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalAppointments;

public class GetPortalAppointmentsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetPortalAppointmentsQuery, Result<List<PortalAppointmentDto>>>
{
    public async Task<Result<List<PortalAppointmentDto>>> Handle(GetPortalAppointmentsQuery query, CancellationToken ct)
    {
        var appointments = await uow.Appointments.FindAsync(
            a => a.PatientId == query.PatientId &&
                 (!query.UpcomingOnly || a.ScheduledDate >= DateTime.UtcNow),
            ct);

        var result = appointments
            .OrderByDescending(a => a.ScheduledDate)
            .Select(a => new PortalAppointmentDto(
                a.Id,
                a.ScheduledDate,
                a.DurationMinutes,
                a.Type.ToString(),
                a.Status.ToString(),
                string.Empty,
                string.Empty,
                a.Reason,
                a.Notes
            ))
            .ToList();

        return Result<List<PortalAppointmentDto>>.Success(result);
    }
}
