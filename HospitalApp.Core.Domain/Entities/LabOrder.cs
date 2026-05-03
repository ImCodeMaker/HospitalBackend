using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class LabOrder : SharedEntity
{
    public required Guid ConsultId { get; set; }
    public required Guid PatientId { get; set; }
    public required Guid OrderedByDoctorId { get; set; }
    public Guid? AssignedToLabTechId { get; set; }

    public required string TestName { get; set; }
    public string? TestCode { get; set; }
    public string? TestCategory { get; set; } // hematology, biochemistry, imaging, etc.
    public required LabTestPriorityEnum Priority { get; set; }
    public string? ClinicalIndication { get; set; }
    public string? SampleType { get; set; }

    public bool IsExternal { get; set; }
    public string? ExternalLabName { get; set; }

    // Status: Pending, InProgress, Complete
    public required string Status { get; set; } = "Pending";
    public DateTime? SampleCollectedAt { get; set; }
    public DateTime? ResultsAvailableAt { get; set; }
    public bool ResultReviewedByDoctor { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public Consult? Consult { get; set; }
    public Patient? Patient { get; set; }
    public ICollection<LabResult> Results { get; set; } = [];
}
