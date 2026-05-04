using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Pacs.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Pacs.Queries.GetDicomStudiesByConsult;

public class GetDicomStudiesByConsultQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetDicomStudiesByConsultQuery, Result<List<DicomStudyDto>>>
{
    public async Task<Result<List<DicomStudyDto>>> Handle(GetDicomStudiesByConsultQuery query, CancellationToken ct)
    {
        var studies = await uow.DicomStudies.FindAsync(s => s.ConsultId == query.ConsultId, ct);

        var result = studies
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new DicomStudyDto
            {
                Id = s.Id,
                ConsultId = s.ConsultId,
                UploadedByUserId = s.UploadedByUserId,
                OriginalFileName = s.OriginalFileName,
                FileSizeBytes = s.FileSizeBytes,
                StudyInstanceUid = s.StudyInstanceUid,
                AccessionNumber = s.AccessionNumber,
                Modality = s.Modality,
                StudyDate = s.StudyDate,
                Description = s.Description,
                PatientPosition = s.PatientPosition,
                CreatedAt = s.CreatedAt,
            })
            .ToList();

        return Result<List<DicomStudyDto>>.Success(result);
    }
}
