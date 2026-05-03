using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Appointments.DTOs;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery(
    Guid? DoctorId,
    Guid? PatientId,
    DateTime? From,
    DateTime? To,
    AppointmentStatusEnum? Status,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PaginatedResult<AppointmentDto>>>;
