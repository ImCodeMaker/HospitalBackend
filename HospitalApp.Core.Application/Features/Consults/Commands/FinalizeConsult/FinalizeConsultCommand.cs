using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Commands.FinalizeConsult;

public record FinalizeConsultCommand(Guid ConsultId, Guid DoctorId) : IRequest<Result<Guid>>;
