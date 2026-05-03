namespace HospitalApp.Core.Application.Common;

public interface IDashboardNotifier
{
    Task NotifyAppointmentChangedAsync(CancellationToken ct = default);
    Task NotifyLabOrderChangedAsync(CancellationToken ct = default);
    Task NotifyBillingChangedAsync(CancellationToken ct = default);
    Task NotifyInventoryChangedAsync(CancellationToken ct = default);
    Task NotifyCajaChangedAsync(CancellationToken ct = default);
    Task NotifyCriticalLabResultAsync(Guid doctorUserId, Guid labOrderId, string testName, string value, CancellationToken ct = default);
}
