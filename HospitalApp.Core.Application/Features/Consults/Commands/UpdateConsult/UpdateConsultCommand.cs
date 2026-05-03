using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Commands.UpdateConsult;

public record UpdateConsultCommand(Guid ConsultId, UpdateConsultRequest Request, Guid DoctorId)
    : IRequest<Result<Guid>>;
