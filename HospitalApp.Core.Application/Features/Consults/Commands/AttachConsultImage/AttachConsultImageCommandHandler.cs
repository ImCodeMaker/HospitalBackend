using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Commands.AttachConsultImage;

public class AttachConsultImageCommandHandler(IUnitOfWork uow)
    : IRequestHandler<AttachConsultImageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AttachConsultImageCommand command, CancellationToken ct)
    {
        var consult = await uow.Consults.GetByIdAsync(command.Request.ConsultId, ct);
        if (consult is null)
            return Result<Guid>.NotFound("Consult not found.");

        var req = command.Request;

        var image = new ConsultImage
        {
            ConsultId = req.ConsultId,
            FilePath = req.FilePath,
            FileName = Path.GetFileName(req.FilePath),
            Caption = req.Description,
            UploadedByUserId = command.UploadedByUserId,
        };

        await uow.ConsultImages.AddAsync(image, ct);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Created(image.Id);
    }
}
