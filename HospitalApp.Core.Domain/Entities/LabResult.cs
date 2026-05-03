using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class LabResult : SharedEntity
{
    public required Guid LabOrderId { get; set; }
    public required Guid EnteredByUserId { get; set; }

    public required string TestName { get; set; }
    public string? Value { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public LabResultFlagEnum Flag { get; set; } = LabResultFlagEnum.Normal;
    public string? Notes { get; set; }
    public string? ResultFilePath { get; set; } // PDF/image of full report

    public LabOrder? LabOrder { get; set; }
}
