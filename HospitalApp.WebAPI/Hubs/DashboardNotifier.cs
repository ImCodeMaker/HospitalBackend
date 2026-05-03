using HospitalApp.Core.Application.Common;
using Microsoft.AspNetCore.SignalR;

namespace HospitalApp.WebAPI.Hubs;

public class DashboardNotifier(IHubContext<DashboardHub> hub) : IDashboardNotifier
{
    public Task NotifyAppointmentChangedAsync(CancellationToken ct = default) =>
        hub.Clients.Groups("Admin", "Doctor", "Receptionist")
            .SendAsync("AppointmentChanged", cancellationToken: ct);

    public Task NotifyLabOrderChangedAsync(CancellationToken ct = default) =>
        hub.Clients.Groups("Admin", "Doctor", "LabTechnician")
            .SendAsync("LabOrderChanged", cancellationToken: ct);

    public Task NotifyBillingChangedAsync(CancellationToken ct = default) =>
        hub.Clients.Groups("Admin", "Receptionist")
            .SendAsync("BillingChanged", cancellationToken: ct);

    public Task NotifyInventoryChangedAsync(CancellationToken ct = default) =>
        hub.Clients.Groups("Admin", "Doctor", "Nurse")
            .SendAsync("InventoryChanged", cancellationToken: ct);

    public Task NotifyCajaChangedAsync(CancellationToken ct = default) =>
        hub.Clients.Groups("Admin", "Receptionist")
            .SendAsync("CajaChanged", cancellationToken: ct);

    public Task NotifyCriticalLabResultAsync(Guid doctorUserId, Guid labOrderId, string testName, string value, CancellationToken ct = default) =>
        hub.Clients.User(doctorUserId.ToString())
            .SendAsync("CriticalLabResult", new { labOrderId, testName, value }, cancellationToken: ct);
}
