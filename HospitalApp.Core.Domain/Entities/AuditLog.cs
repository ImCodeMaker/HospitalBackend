namespace HospitalApp.Core.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public required string TableName { get; set; }
    public required Guid RecordId { get; set; }
    public required string Action { get; set; } // INSERT | UPDATE | DELETE
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
