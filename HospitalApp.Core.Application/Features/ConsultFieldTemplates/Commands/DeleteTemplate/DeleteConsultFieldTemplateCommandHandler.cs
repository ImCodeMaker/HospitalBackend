using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.ConsultFieldTemplates.Commands.DeleteTemplate;

public class DeleteConsultFieldTemplateCommandHandler(IUnitOfWork uow)
    : IRequestHandler<DeleteConsultFieldTemplateCommand, Result>
{
    public async Task<Result> Handle(DeleteConsultFieldTemplateCommand command, CancellationToken ct)
    {
        var template = await uow.ConsultFieldTemplates.GetByIdAsync(command.TemplateId, ct);
        if (template is null)
            return Result.NotFound("Consult field template not found.");

        template.UpdatedAt = DateTime.UtcNow;

        uow.ConsultFieldTemplates.Delete(template);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
