using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Features.Recruitment.DTOs;

public class RecruitmentApplicationDto
{
    public Guid Id { get; set; }
    public string ApplicantName { get; set; } = default!;
    public string Position { get; set; } = default!;
    public string Stage { get; set; } = default!;
    public DateTime AppliedDate { get; set; }
    public DateTime? InterviewDate { get; set; }
    public string? Notes { get; set; }
    public string? CvFilePath { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateRecruitmentApplicationRequest(
    string ApplicantName,
    string Position,
    string? Notes,
    string? CvFilePath);

public record AdvanceRecruitmentStageRequest(
    RecruitmentStageEnum NewStage,
    DateTime? InterviewDate,
    string? Notes);
