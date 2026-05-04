using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Pacs.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Pacs.Queries.GetDicomStudyById;

public record GetDicomStudyByIdQuery(Guid StudyId) : IRequest<Result<DicomStudyDto>>;
