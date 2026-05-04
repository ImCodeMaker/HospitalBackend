using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Pacs.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Pacs.Queries.GetDicomStudiesByConsult;

public record GetDicomStudiesByConsultQuery(Guid ConsultId) : IRequest<Result<List<DicomStudyDto>>>;
