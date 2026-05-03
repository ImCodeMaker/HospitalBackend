namespace HospitalApp.Core.Application.Features.Auth.DTOs;

public record TotpSetupDto(string Secret, string QrCodeUri, string ManualEntryKey);

public record EnableTotpRequest(string TotpCode);

public record DisableTotpRequest(string CurrentPassword);

public record LoginWithTotpRequest(string Email, string Password, string TotpCode);
