using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Pacs.Commands.UploadDicomStudy;

public class UploadDicomStudyCommandHandler(
    IUnitOfWork uow,
    IFileStorageService fileStorage,
    IMalwareScanner malwareScanner)
    : IRequestHandler<UploadDicomStudyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UploadDicomStudyCommand command, CancellationToken ct)
    {
        var consult = await uow.Consults.GetByIdAsync(command.ConsultId, ct);
        if (consult is null)
            return Result<Guid>.NotFound("Consult not found.");

        var scan = await malwareScanner.ScanAsync(command.FileStream, command.OriginalFileName, ct);
        if (!scan.IsClean)
            return Result<Guid>.Failure(
                $"File failed malware scan. {scan.Signature ?? scan.Error}",
                400);

        var filePath = await fileStorage.SaveAsync(
            command.FileStream,
            command.OriginalFileName,
            $"pacs/{command.ConsultId}",
            ct);

        var study = new DicomStudy
        {
            ConsultId = command.ConsultId,
            UploadedByUserId = command.UploadedByUserId,
            FilePath = filePath,
            OriginalFileName = command.OriginalFileName,
            FileSizeBytes = command.FileSizeBytes,
            Modality = command.Modality,
            StudyInstanceUid = command.StudyInstanceUid,
            AccessionNumber = command.AccessionNumber,
            StudyDate = command.StudyDate,
            Description = command.Description,
        };

        await uow.DicomStudies.AddAsync(study, ct);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Created(study.Id);
    }
}
