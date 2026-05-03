using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Auth.DTOs;

namespace HospitalApp.Core.Application.Features.Auth.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default);
    Task<Result<Guid>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result> RevokeTokenAsync(Guid userId, CancellationToken ct = default);

    Task<Result<TotpSetupDto>> GenerateTotpSetupAsync(Guid userId, CancellationToken ct = default);
    Task<Result> EnableTotpAsync(Guid userId, string totpCode, CancellationToken ct = default);
    Task<Result> DisableTotpAsync(Guid userId, string currentPassword, CancellationToken ct = default);
    Task<Result<AuthResponse>> LoginWithTotpAsync(string email, string password, string totpCode, CancellationToken ct = default);
}
