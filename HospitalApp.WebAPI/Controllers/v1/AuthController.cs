using Asp.Versioning;
using HospitalApp.Core.Application.Features.Auth.DTOs;
using HospitalApp.Core.Application.Features.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(IAuthService authService) : BaseController
{
    private const string RefreshTokenCookie = "refresh_token";

    /// <summary>Authenticate and receive JWT access token. Refresh token set as HTTP-only cookie.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        SetRefreshCookie(result.Data!.RefreshToken);
        return Ok(new
        {
            result.Data.AccessToken,
            result.Data.ExpiresAt,
            result.Data.UserId,
            result.Data.Email,
            result.Data.FullName,
            result.Data.Roles,
            result.Data.SpecialtyId,
        });
    }

    /// <summary>Register a new user (Admin only).</summary>
    [HttpPost("register")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Login), new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Refresh access token using HTTP-only cookie (falls back to body).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest? request, CancellationToken ct)
    {
        // prefer cookie; fall back to body for clients that can't use cookies
        var cookieToken = Request.Cookies[RefreshTokenCookie];
        var accessToken = request?.AccessToken ?? string.Empty;
        var refreshToken = cookieToken ?? request?.RefreshToken ?? string.Empty;

        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(new { error = "Refresh token required." });

        var result = await authService.RefreshTokenAsync(accessToken, refreshToken, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        SetRefreshCookie(result.Data!.RefreshToken);
        return Ok(new { result.Data.AccessToken, result.Data.ExpiresAt });
    }

    /// <summary>Revoke the current user's refresh token and clear cookie.</summary>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await authService.RevokeTokenAsync(userId, ct);
        Response.Cookies.Delete(RefreshTokenCookie);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    // ──────────────────────────────────────────────
    // 2FA / TOTP endpoints
    // ──────────────────────────────────────────────

    /// <summary>Generate TOTP secret and QR code URI (first step of 2FA setup).</summary>
    [HttpPost("2fa/setup")]
    [Authorize]
    public async Task<IActionResult> TotpSetup(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await authService.GenerateTotpSetupAsync(userId, ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Confirm TOTP code and enable 2FA for the current user.</summary>
    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<IActionResult> TotpEnable([FromBody] EnableTotpRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await authService.EnableTotpAsync(userId, request.TotpCode, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Disable 2FA (requires current password confirmation).</summary>
    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> TotpDisable([FromBody] DisableTotpRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await authService.DisableTotpAsync(userId, request.CurrentPassword, ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Complete login when 2FA is required (email + password + TOTP code).</summary>
    [HttpPost("2fa/login")]
    [AllowAnonymous]
    public async Task<IActionResult> TotpLogin([FromBody] LoginWithTotpRequest request, CancellationToken ct)
    {
        var result = await authService.LoginWithTotpAsync(request.Email, request.Password, request.TotpCode, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        SetRefreshCookie(result.Data!.RefreshToken);
        return Ok(new
        {
            result.Data.AccessToken,
            result.Data.ExpiresAt,
            result.Data.UserId,
            result.Data.Email,
            result.Data.FullName,
            result.Data.Roles,
            result.Data.SpecialtyId,
        });
    }

    private void SetRefreshCookie(string token)
    {
        Response.Cookies.Append(RefreshTokenCookie, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            Path = "/api",
        });
    }
}

public record RefreshTokenRequest(string AccessToken, string? RefreshToken);
