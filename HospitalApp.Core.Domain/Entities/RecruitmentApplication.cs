using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class RecruitmentApplication : SharedEntity
{
    public required string ApplicantName { get; set; }
    public required string Position { get; set; }
    public required DateTime AppliedDate { get; set; }
    public RecruitmentStageEnum Stage { get; set; } = RecruitmentStageEnum.Applicant;
    public string? CvFilePath { get; set; }
    public DateTime? InterviewDate { get; set; }
    public Guid? InterviewerId { get; set; }
    public int? InterviewScore { get; set; }
    public string? InterviewNotes { get; set; }
    public decimal? OfferAmount { get; set; }
    public DateTime? OfferDate { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? ConvertedToEmployeeId { get; set; }

    public Employee? ConvertedToEmployee { get; set; }
}
