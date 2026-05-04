using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Pacs.Commands.DeleteDicomStudy;

public record DeleteDicomStudyCommand(Guid StudyId) : IRequest<Result>;
