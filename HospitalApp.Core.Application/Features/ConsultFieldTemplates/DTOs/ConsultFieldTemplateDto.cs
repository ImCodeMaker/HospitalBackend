namespace HospitalApp.Core.Application.Features.ConsultFieldTemplates.DTOs;

public class ConsultFieldTemplateDto
{
    public Guid Id { get; init; }
    public Guid SpecialtyId { get; init; }
    public string FieldKey { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string FieldType { get; init; } = string.Empty; // text, number, select, checkbox, date, textarea
    public string? FieldOptions { get; init; } // JSON array for select type
    public bool IsRequired { get; init; }
    public int DisplayOrder { get; init; }
}

public record CreateConsultFieldTemplateRequest(
    Guid SpecialtyId,
    string FieldKey,
    string Label,
    string FieldType,
    string? FieldOptions,
    bool IsRequired,
    int DisplayOrder);

public record UpdateConsultFieldTemplateRequest(
    string? Label,
    string? FieldOptions,
    bool? IsRequired,
    int? DisplayOrder);
