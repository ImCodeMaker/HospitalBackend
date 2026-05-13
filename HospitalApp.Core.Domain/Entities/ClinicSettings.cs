namespace HospitalApp.Core.Domain.Entities;

public class ClinicSettings : SharedEntity
{
    public required string ClinicName { get; set; }
    public string? Rnc { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? LogoPath { get; set; }
    public string TimeZone { get; set; } = "America/Santo_Domingo";
    public string Currency { get; set; } = "DOP";
    public decimal ItbisRate { get; set; } = 0.18m;
    public string? OperatingHours { get; set; } // JSONB: per-day schedule
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool SmsNotificationsEnabled { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPasswordEncrypted { get; set; }
    public string? TwilioAccountSid { get; set; }
    public string? TwilioAuthTokenEncrypted { get; set; }
    public string? TwilioFromNumber { get; set; }
    public int SessionTimeoutMinutes { get; set; } = 15;
    /// <summary>Minimum gap (minutes) the system enforces between adjacent appointments for the same doctor.</summary>
    public int AppointmentBufferMinutes { get; set; } = 10;
    /// <summary>Number of no-show outreach contacts (within 90 days) before a patient is flagged as repeat-offender.</summary>
    public int NoShowRepeatOffenderThreshold { get; set; } = 3;
}
