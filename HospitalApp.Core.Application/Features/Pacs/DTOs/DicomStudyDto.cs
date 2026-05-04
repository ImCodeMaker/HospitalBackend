namespace HospitalApp.Core.Application.Features.Pacs.DTOs;

public class DicomStudyDto
{
    public Guid Id { get; init; }
    public Guid ConsultId { get; init; }
    public Guid UploadedByUserId { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string? StudyInstanceUid { get; init; }
    public string? AccessionNumber { get; init; }
    public string? Modality { get; init; }
    public DateTime? StudyDate { get; init; }
    public string? Description { get; init; }
    public string? PatientPosition { get; init; }
    public DateTime CreatedAt { get; init; }
}
