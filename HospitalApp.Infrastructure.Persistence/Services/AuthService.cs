using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Auth.DTOs;
using HospitalApp.Core.Application.Features.Auth.Services;
using HospitalApp.Infrastructure.Identity.Entities;
using HospitalApp.Infrastructure.Identity.Services;
using HospitalApp.Infrastructure.Identity.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OtpNet;

namespace HospitalApp.Infrastructure.Persistence.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            return Result<AuthResponse>.Unauthorized("Invalid credentials.");

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);
            return Result<AuthResponse>.Unauthorized("Invalid credentials.");
        }

        if (await userManager.IsLockedOutAsync(user))
            return Result<AuthResponse>.Unauthorized("Account locked. Try again in 15 minutes.");

        // If TOTP is enabled, signal the client to complete the 2FA step
        if (user.IsTotpEnabled)
            return Result<AuthResponse>.Failure("2FA_REQUIRED", 202);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
        user.LastLoginAt = DateTime.UtcNow;
        await userManager.ResetAccessFailedCountAsync(user);
        await userManager.UpdateAsync(user);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Roles = roles,
            SpecialtyId = user.SpecialtyId
        });
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default)
    {
        var principal = jwtTokenService.GetPrincipalFromExpiredToken(accessToken);
        if (principal is null)
            return Result<AuthResponse>.Unauthorized("Invalid access token.");

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (userId is null || !Guid.TryParse(userId, out var parsedId))
            return Result<AuthResponse>.Unauthorized("Invalid token claims.");

        var user = await userManager.FindByIdAsync(parsedId.ToString());
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.UtcNow)
            return Result<AuthResponse>.Unauthorized("Invalid or expired refresh token.");

        var roles = await userManager.GetRolesAsync(user);
        var newAccessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
        await userManager.UpdateAsync(user);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Roles = roles,
            SpecialtyId = user.SpecialtyId
        });
    }

    public async Task<Result<Guid>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Result<Guid>.Failure("Email already registered.", 409);

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
            return Result<Guid>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));

        if (!string.IsNullOrEmpty(request.Role))
            await userManager.AddToRoleAsync(user, request.Role);

        return Result<Guid>.Created(user.Id);
    }

    public async Task<Result> RevokeTokenAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.NotFound("User not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result<TotpSetupDto>> GenerateTotpSetupAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result<TotpSetupDto>.NotFound("User not found.");

        // Generate a cryptographically random 20-byte secret and encode as Base32
        var secretBytes = RandomNumberGenerator.GetBytes(20);
        var secret = Base32Encoding.ToString(secretBytes);

        // Store pending secret via Identity token storage (not persisted to TotpSecret until confirmed)
        await userManager.SetAuthenticationTokenAsync(user, "Totp", "PendingSecret", secret);

        var email = user.Email ?? user.UserName ?? userId.ToString();
        var qrCodeUri = $"otpauth://totp/LovaSalud:{Uri.EscapeDataString(email)}?secret={secret}&issuer=LovaSalud";

        return Result<TotpSetupDto>.Success(new TotpSetupDto(secret, qrCodeUri, secret));
    }

    public async Task<Result> EnableTotpAsync(Guid userId, string totpCode, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.NotFound("User not found.");

        var pendingSecret = await userManager.GetAuthenticationTokenAsync(user, "Totp", "PendingSecret");
        if (string.IsNullOrEmpty(pendingSecret))
            return Result.Failure("No pending TOTP setup found. Call /auth/2fa/setup first.", 400);

        var totp = new Totp(Base32Encoding.ToBytes(pendingSecret));
        bool valid = totp.VerifyTotp(totpCode, out _, new VerificationWindow(1, 1));
        if (!valid)
            return Result.Failure("Invalid TOTP code.", 400);

        user.TotpSecret = pendingSecret;
        user.IsTotpEnabled = true;
        await userManager.RemoveAuthenticationTokenAsync(user, "Totp", "PendingSecret");
        await userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result> DisableTotpAsync(Guid userId, string currentPassword, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.NotFound("User not found.");

        if (!await userManager.CheckPasswordAsync(user, currentPassword))
            return Result.Failure("Invalid password.", 400);

        user.TotpSecret = null;
        user.IsTotpEnabled = false;
        await userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result<AuthResponse>> LoginWithTotpAsync(string email, string password, string totpCode, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
            return Result<AuthResponse>.Unauthorized("Invalid credentials.");

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return Result<AuthResponse>.Unauthorized("Invalid credentials.");
        }

        if (await userManager.IsLockedOutAsync(user))
            return Result<AuthResponse>.Unauthorized("Account locked. Try again in 15 minutes.");

        if (!user.IsTotpEnabled || string.IsNullOrEmpty(user.TotpSecret))
            return Result<AuthResponse>.Failure("2FA is not enabled for this account.", 400);

        var totp = new Totp(Base32Encoding.ToBytes(user.TotpSecret));
        bool valid = totp.VerifyTotp(totpCode, out _, new VerificationWindow(1, 1));
        if (!valid)
            return Result<AuthResponse>.Unauthorized("Invalid TOTP code.");

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
        user.LastLoginAt = DateTime.UtcNow;
        await userManager.ResetAccessFailedCountAsync(user);
        await userManager.UpdateAsync(user);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Roles = roles,
            SpecialtyId = user.SpecialtyId
        });
    }
}
