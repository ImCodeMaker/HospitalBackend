using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.UserManagement.DTOs;
using HospitalApp.Core.Application.Features.UserManagement.Services;
using HospitalApp.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace HospitalApp.Infrastructure.Identity.Services;

public class UserManagementService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : IUserManagementService
{
    public async Task<Result<PaginatedResult<UserDto>>> GetUsersAsync(
        string? roleFilter, bool? activeOnly, int page, int pageSize, CancellationToken ct)
    {
        var users = userManager.Users.AsEnumerable();

        if (activeOnly.HasValue)
            users = users.Where(u => u.IsActive == activeOnly.Value);

        var userList = users.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToList();

        var dtos = new List<UserDto>();
        foreach (var user in userList)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roleFilter != null && !roles.Contains(roleFilter)) continue;
            var isLockedOut = await userManager.IsLockedOutAsync(user);
            dtos.Add(MapToDto(user, roles, isLockedOut));
        }

        var total = dtos.Count;
        var paged = dtos.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Result<PaginatedResult<UserDto>>.Success(
            PaginatedResult<UserDto>.Create(paged, total, page, pageSize));
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result<UserDto>.NotFound("User not found.");

        var roles = await userManager.GetRolesAsync(user);
        var isLockedOut = await userManager.IsLockedOutAsync(user);
        return Result<UserDto>.Success(MapToDto(user, roles, isLockedOut));
    }

    public async Task<Result<Guid>> CreateUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        if (!await roleManager.RoleExistsAsync(request.Role))
            return Result<Guid>.Failure($"Role '{request.Role}' does not exist.", 400);

        var user = new ApplicationUser
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email,
            SpecialtyId = request.SpecialtyId,
            IsActive = true,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result<Guid>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)), 400);

        await userManager.AddToRoleAsync(user, request.Role);
        return Result<Guid>.Created(user.Id);
    }

    public async Task<Result> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.NotFound("User not found.");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.SpecialtyId.HasValue) user.SpecialtyId = request.SpecialtyId;

        if (request.Email is not null && user.Email != request.Email)
        {
            user.Email = request.Email;
            user.UserName = request.Email;
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Result.Failure(string.Join("; ", updateResult.Errors.Select(e => e.Description)), 400);

        if (request.Role is not null)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, request.Role);
        }

        return Result.Success();
    }

    public async Task<Result> DeactivateUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.NotFound("User not found.");
        user.IsActive = false;
        await userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result> ActivateUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.NotFound("User not found.");
        user.IsActive = true;
        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Result.NotFound("User not found.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(string.Join("; ", result.Errors.Select(e => e.Description)), 400);
    }

    private static UserDto MapToDto(ApplicationUser user, IList<string> roles, bool isLockedOut) =>
        new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Roles = [.. roles],
            SpecialtyId = user.SpecialtyId,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            IsLockedOut = isLockedOut,
        };
}
