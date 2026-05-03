namespace HospitalApp.Core.Application.Features.LabOrders.DTOs;

public class LabOrderDto
{
    public Guid Id { get; init; }
    public Guid ConsultId { get; init; }
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public Guid OrderedByDoctorId { get; init; }
    public string DoctorName { get; init; } = string.Empty;
    public string TestName { get; init; } = string.Empty;
    public string? TestCategory { get; init; }
    public string Priority { get; init; } = string.Empty;
    public string? ClinicalIndication { get; init; }
    public string? SampleType { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsExternal { get; init; }
    public string? ExternalLabName { get; init; }
    public DateTime? SampleCollectedAt { get; init; }
    public DateTime? ResultsAvailableAt { get; init; }
    public bool ResultReviewedByDoctor { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<LabResultDto> Results { get; init; } = [];
}

public class LabResultDto
{
    public Guid Id { get; init; }
    public string TestName { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string? Unit { get; init; }
    public string? ReferenceRange { get; init; }
    public string Flag { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public string? ResultFilePath { get; init; }
}
