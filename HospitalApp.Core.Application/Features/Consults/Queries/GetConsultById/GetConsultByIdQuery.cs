using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Queries.GetConsultById;

public record GetConsultByIdQuery(Guid ConsultId) : IRequest<Result<ConsultDto>>;
