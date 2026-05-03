using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.UserManagement.DTOs;

namespace HospitalApp.Core.Application.Features.UserManagement.Services;

public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    Guid? SpecialtyId
);

public record UpdateUserRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Role,
    Guid? SpecialtyId
);

public interface IUserManagementService
{
    Task<Result<PaginatedResult<UserDto>>> GetUsersAsync(string? roleFilter, bool? activeOnly, int page, int pageSize, CancellationToken ct);
    Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken ct);
    Task<Result<Guid>> CreateUserAsync(CreateUserRequest request, CancellationToken ct);
    Task<Result> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken ct);
    Task<Result> DeactivateUserAsync(Guid userId, CancellationToken ct);
    Task<Result> ActivateUserAsync(Guid userId, CancellationToken ct);
    Task<Result> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken ct);
}
