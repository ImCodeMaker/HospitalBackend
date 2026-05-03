namespace HospitalApp.Core.Domain.Entities;

public class ConsultFieldTemplate : SharedEntity
{
    public required Guid SpecialtyId { get; set; }
    public required string FieldKey { get; set; }
    public required string FieldLabel { get; set; }
    public required string FieldType { get; set; } // text|number|boolean|enum|date|file|multi_select
    public string? FieldOptions { get; set; } // JSONB: enum values, validation rules
    public bool IsRequired { get; set; }
    public int DisplayOrder { get; set; }
    public string? SectionName { get; set; }
    public bool IsActive { get; set; } = true;

    public Specialty? Specialty { get; set; }
}
