namespace HospitalApp.Core.Application.Common;
public interface ISmsService
{
    Task SendAsync(string toPhoneE164, string message, CancellationToken ct = default);
}
