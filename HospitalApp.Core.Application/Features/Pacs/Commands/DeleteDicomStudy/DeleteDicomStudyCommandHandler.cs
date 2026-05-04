using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Pacs.Commands.DeleteDicomStudy;

public class DeleteDicomStudyCommandHandler(IUnitOfWork uow, IFileStorageService fileStorage)
    : IRequestHandler<DeleteDicomStudyCommand, Result>
{
    public async Task<Result> Handle(DeleteDicomStudyCommand command, CancellationToken ct)
    {
        var study = await uow.DicomStudies.GetByIdAsync(command.StudyId, ct);
        if (study is null)
            return Result.NotFound("DICOM study not found.");

        await fileStorage.DeleteAsync(study.FilePath, ct);
        uow.DicomStudies.Delete(study);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
