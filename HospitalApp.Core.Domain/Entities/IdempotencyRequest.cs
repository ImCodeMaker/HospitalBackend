namespace HospitalApp.Core.Domain.Entities;

public class IdempotencyRequest : SharedEntity
{
    public required string Key { get; set; }
    public required string RequestHash { get; set; }
    public required string Method { get; set; }
    public required string Path { get; set; }
    public Guid? UserId { get; set; }
    public required string Status { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseContentType { get; set; }
    public string? ResponseLocation { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
