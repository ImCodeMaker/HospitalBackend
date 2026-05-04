using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Pacs.Commands.UploadDicomStudy;

public record UploadDicomStudyCommand(
    Guid ConsultId,
    Guid UploadedByUserId,
    Stream FileStream,
    string OriginalFileName,
    long FileSizeBytes,
    string? Modality,
    string? StudyInstanceUid,
    string? AccessionNumber,
    DateTime? StudyDate,
    string? Description
) : IRequest<Result<Guid>>;
