using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Pacs.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Pacs.Queries.GetDicomStudyById;

public class GetDicomStudyByIdQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetDicomStudyByIdQuery, Result<DicomStudyDto>>
{
    public async Task<Result<DicomStudyDto>> Handle(GetDicomStudyByIdQuery query, CancellationToken ct)
    {
        var study = await uow.DicomStudies.GetByIdAsync(query.StudyId, ct);
        if (study is null)
            return Result<DicomStudyDto>.NotFound("DICOM study not found.");

        return Result<DicomStudyDto>.Success(new DicomStudyDto
        {
            Id = study.Id,
            ConsultId = study.ConsultId,
            UploadedByUserId = study.UploadedByUserId,
            FilePath = study.FilePath,
            OriginalFileName = study.OriginalFileName,
            FileSizeBytes = study.FileSizeBytes,
            StudyInstanceUid = study.StudyInstanceUid,
            AccessionNumber = study.AccessionNumber,
            Modality = study.Modality,
            StudyDate = study.StudyDate,
            Description = study.Description,
            PatientPosition = study.PatientPosition,
            CreatedAt = study.CreatedAt,
        });
    }
}
