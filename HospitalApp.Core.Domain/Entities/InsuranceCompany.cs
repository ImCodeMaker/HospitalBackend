namespace HospitalApp.Core.Domain.Entities;

public class InsuranceCompany : SharedEntity
{
    public required string Name { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? ClaimSubmissionInstructions { get; set; }
    public decimal DefaultCoveragePercentage { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Patient> Patients { get; set; } = [];
}
