namespace HospitalApp.Core.Application.Common;

/// <summary>
/// Sends WhatsApp messages via Twilio's WhatsApp Business channel.
/// Stub today — wire real Twilio account via TwilioSettings + production WA-approved templates.
/// </summary>
public interface IWhatsAppService
{
    Task<bool> SendAsync(string toPhoneE164, string templateName, IDictionary<string, string> parameters, CancellationToken ct = default);
    Task<bool> SendFreeformAsync(string toPhoneE164, string body, CancellationToken ct = default);
}
