namespace HospitalApp.Core.Application.Features.Settings.DTOs;

public class ClinicSettingsDto
{
    public Guid Id { get; init; }
    public string ClinicName { get; init; } = string.Empty;
    public string? Rnc { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? LogoPath { get; init; }
    public string TimeZone { get; init; } = "America/Santo_Domingo";
    public string Currency { get; init; } = "DOP";
    public decimal ItbisRate { get; init; }
    public bool EmailNotificationsEnabled { get; init; }
    public bool SmsNotificationsEnabled { get; init; }
    public int SessionTimeoutMinutes { get; init; }
}
