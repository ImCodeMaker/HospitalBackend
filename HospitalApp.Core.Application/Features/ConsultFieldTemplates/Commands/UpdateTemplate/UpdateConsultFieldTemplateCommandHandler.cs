using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.ConsultFieldTemplates.Commands.UpdateTemplate;

public class UpdateConsultFieldTemplateCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateConsultFieldTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateConsultFieldTemplateCommand command, CancellationToken ct)
    {
        var template = await uow.ConsultFieldTemplates.GetByIdAsync(command.TemplateId, ct);
        if (template is null)
            return Result<Guid>.NotFound("Consult field template not found.");

        var req = command.Request;

        if (req.Label is not null) template.FieldLabel = req.Label;
        if (req.FieldOptions is not null) template.FieldOptions = req.FieldOptions;
        if (req.IsRequired.HasValue) template.IsRequired = req.IsRequired.Value;
        if (req.DisplayOrder.HasValue) template.DisplayOrder = req.DisplayOrder.Value;
        template.UpdatedAt = DateTime.UtcNow;

        uow.ConsultFieldTemplates.Update(template);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(template.Id);
    }
}
