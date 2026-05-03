namespace HospitalApp.Core.Application.Features.Auth.DTOs;

public class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public IList<string> Roles { get; init; } = [];
    public Guid? SpecialtyId { get; init; }
}
