using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Queries.GetConsultImages;

public record GetConsultImagesQuery(Guid ConsultId) : IRequest<Result<List<ConsultImageDto>>>;
