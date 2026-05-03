using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Appointments.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand(CreateAppointmentRequest Request, Guid ScheduledByUserId)
    : IRequest<Result<Guid>>;
