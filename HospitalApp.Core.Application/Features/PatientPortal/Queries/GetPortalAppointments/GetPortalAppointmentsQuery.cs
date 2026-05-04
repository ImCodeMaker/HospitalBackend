using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.PatientPortal.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalAppointments;

public record GetPortalAppointmentsQuery(Guid PatientId, bool UpcomingOnly = false)
    : IRequest<Result<List<PortalAppointmentDto>>>;
