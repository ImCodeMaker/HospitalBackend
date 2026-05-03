using HospitalApp.Core.Application.Common;
using HospitalApp.Infrastructure.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HospitalApp.Infrastructure.Shared.Services;

public class TwilioSmsService(IOptions<TwilioSettings> opts, ILogger<TwilioSmsService> logger)
    : ISmsService
{
    private readonly TwilioSettings _cfg = opts.Value;

    public async Task SendAsync(string toPhoneE164, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_cfg.AccountSid) || string.IsNullOrEmpty(_cfg.AuthToken))
        {
            logger.LogWarning("Twilio is not configured. SMS to {To} skipped.", toPhoneE164);
            return;
        }

        try
        {
            TwilioClient.Init(_cfg.AccountSid, _cfg.AuthToken);
            await MessageResource.CreateAsync(
                to: new PhoneNumber(toPhoneE164),
                from: new PhoneNumber(_cfg.FromNumber),
                body: message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send SMS to {To}.", toPhoneE164);
            // swallow — SMS is non-blocking
        }
    }
}
