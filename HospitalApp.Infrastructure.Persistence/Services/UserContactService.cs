using HospitalApp.Core.Application.Common;
using HospitalApp.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.Infrastructure.Persistence.Services;

public class UserContactService(ApplicationDbContext db) : IUserContactService
{
    public async Task<UserContact?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserContact(u.Id, u.FirstName + " " + u.LastName, u.Email, u.PhoneNumber))
            .FirstOrDefaultAsync(ct);
    }
}
