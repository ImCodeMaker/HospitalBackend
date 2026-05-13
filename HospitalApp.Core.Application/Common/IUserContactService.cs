namespace HospitalApp.Core.Application.Common;

public interface IUserContactService
{
    Task<UserContact?> GetAsync(Guid userId, CancellationToken ct = default);
}

public record UserContact(Guid UserId, string FullName, string? Email, string? Phone);
