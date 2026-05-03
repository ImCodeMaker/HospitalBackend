using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Caja.Commands.OpenShift;

public record OpenShiftCommand(decimal OpeningBalance, Guid UserId) : IRequest<Result<Guid>>;
