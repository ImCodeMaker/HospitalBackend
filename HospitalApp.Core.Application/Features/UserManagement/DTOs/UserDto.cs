namespace HospitalApp.Core.Application.Features.UserManagement.DTOs;

public class UserDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = [];
    public Guid? SpecialtyId { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public bool IsLockedOut { get; init; }
}
