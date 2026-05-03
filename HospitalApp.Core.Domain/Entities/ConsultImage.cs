namespace HospitalApp.Core.Domain.Entities;

public class ConsultImage : SharedEntity
{
    public required Guid ConsultId { get; set; }
    public required string FilePath { get; set; }
    public required string FileName { get; set; }
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Caption { get; set; }
    public Guid UploadedByUserId { get; set; }

    public Consult? Consult { get; set; }
}
