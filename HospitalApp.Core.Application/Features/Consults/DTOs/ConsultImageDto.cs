namespace HospitalApp.Core.Application.Features.Consults.DTOs;

public class ConsultImageDto
{
    public Guid Id { get; init; }
    public Guid ConsultId { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AttachConsultImageRequest(Guid ConsultId, string FilePath, string? Description);
