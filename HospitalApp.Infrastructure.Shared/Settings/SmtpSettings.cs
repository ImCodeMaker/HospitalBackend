namespace HospitalApp.Infrastructure.Shared.Settings;

public class SmtpSettings
{
    public required string Host { get; set; }
    public int Port { get; set; } = 587;
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Lova Salud";
    public bool EnableSsl { get; set; } = true;
}
