using HospitalApp.Infrastructure.Identity.Entities;
using System.Security.Claims;

namespace HospitalApp.Infrastructure.Identity.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
