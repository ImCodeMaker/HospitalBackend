namespace HospitalApp.Core.Application.Features.AuditLog.DTOs;

public class AuditLogDto
{
    public long Id { get; init; }
    public string TableName { get; init; } = string.Empty;
    public Guid RecordId { get; init; }
    public string Action { get; init; } = string.Empty;
    public Guid? ChangedBy { get; init; }
    public DateTime ChangedAt { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string? IpAddress { get; init; }
}
