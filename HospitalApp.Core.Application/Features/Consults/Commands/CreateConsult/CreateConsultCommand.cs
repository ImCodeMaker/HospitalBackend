using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Commands.CreateConsult;

public record CreateConsultCommand(CreateConsultRequest Request, Guid DoctorId) : IRequest<Result<Guid>>;
