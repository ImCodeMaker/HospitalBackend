using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.ConsultFieldTemplates.Commands.CreateTemplate;

public class CreateConsultFieldTemplateCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateConsultFieldTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateConsultFieldTemplateCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var duplicate = await uow.ConsultFieldTemplates.ExistsAsync(
            t => t.SpecialtyId == req.SpecialtyId && t.FieldKey == req.FieldKey, ct);

        if (duplicate)
            return Result<Guid>.Failure(
                $"A template with FieldKey '{req.FieldKey}' already exists for this specialty.", 409);

        var template = new ConsultFieldTemplate
        {
            SpecialtyId = req.SpecialtyId,
            FieldKey = req.FieldKey,
            FieldLabel = req.Label,
            FieldType = req.FieldType,
            FieldOptions = req.FieldOptions,
            IsRequired = req.IsRequired,
            DisplayOrder = req.DisplayOrder
        };

        await uow.ConsultFieldTemplates.AddAsync(template, ct);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Created(template.Id);
    }
}
