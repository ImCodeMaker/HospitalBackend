using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.ConsultFieldTemplates.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.ConsultFieldTemplates.Queries.GetTemplatesBySpecialty;

public class GetTemplatesBySpecialtyQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetTemplatesBySpecialtyQuery, Result<List<ConsultFieldTemplateDto>>>
{
    public async Task<Result<List<ConsultFieldTemplateDto>>> Handle(GetTemplatesBySpecialtyQuery query, CancellationToken ct)
    {
        var templates = await uow.ConsultFieldTemplates.FindAsync(
            t => t.SpecialtyId == query.SpecialtyId, ct);

        var dtos = templates
            .OrderBy(t => t.DisplayOrder)
            .Select(t => new ConsultFieldTemplateDto
            {
                Id = t.Id,
                SpecialtyId = t.SpecialtyId,
                FieldKey = t.FieldKey,
                Label = t.FieldLabel,
                FieldType = t.FieldType,
                FieldOptions = t.FieldOptions,
                IsRequired = t.IsRequired,
                DisplayOrder = t.DisplayOrder
            })
            .ToList();

        return Result<List<ConsultFieldTemplateDto>>.Success(dtos);
    }
}
