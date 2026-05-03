using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Caja.Commands.CloseShift;

public record CloseShiftCommand(Guid ShiftId, decimal ClosingBalance, string? Notes, Guid UserId)
    : IRequest<Result<Guid>>;
