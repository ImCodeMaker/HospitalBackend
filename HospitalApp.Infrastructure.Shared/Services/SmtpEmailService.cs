using System.Net;
using System.Net.Mail;
using HospitalApp.Core.Application.Common;
using HospitalApp.Infrastructure.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HospitalApp.Infrastructure.Shared.Services;

public class SmtpEmailService(IOptions<SmtpSettings> opts, ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly SmtpSettings _cfg = opts.Value;

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default) =>
        SendAsync([to], subject, htmlBody, ct);

    public async Task SendAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken ct = default)
    {
        using var client = new SmtpClient(_cfg.Host, _cfg.Port)
        {
            Credentials = new NetworkCredential(_cfg.UserName, _cfg.Password),
            EnableSsl = _cfg.EnableSsl,
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_cfg.FromAddress, _cfg.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };

        foreach (var address in to)
            message.To.Add(address);

        try
        {
            await client.SendMailAsync(message, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Recipients} with subject {Subject}",
                string.Join(", ", to), subject);
            throw;
        }
    }
}
