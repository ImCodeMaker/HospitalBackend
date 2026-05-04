namespace HospitalApp.Core.Domain.Entities;

public class DicomStudy : SharedEntity
{
    public required Guid ConsultId { get; set; }
    public required Guid UploadedByUserId { get; set; }

    public required string FilePath { get; set; }
    public required string OriginalFileName { get; set; }
    public required long FileSizeBytes { get; set; }

    // DICOM header fields (optional — parsed by uploader if available)
    public string? StudyInstanceUid { get; set; }
    public string? AccessionNumber { get; set; }
    public string? Modality { get; set; }        // CR, DX, CT, MR, US, etc.
    public DateTime? StudyDate { get; set; }
    public string? Description { get; set; }
    public string? PatientPosition { get; set; }

    public Consult? Consult { get; set; }
}
