using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Appointments.Commands.UpdateAppointmentStatus;

public record UpdateAppointmentStatusCommand(Guid AppointmentId, AppointmentStatusEnum NewStatus, string? Notes)
    : IRequest<Result<Guid>>;
