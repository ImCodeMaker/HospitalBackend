using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Settings.Commands.UpdateSettings;

public record UpdateSettingsRequest(
    string ClinicName,
    string? Rnc,
    string? Address,
    string? Phone,
    string? Email,
    string TimeZone,
    string Currency,
    decimal ItbisRate,
    bool EmailNotificationsEnabled,
    bool SmsNotificationsEnabled,
    int SessionTimeoutMinutes
);

public record UpdateSettingsCommand(UpdateSettingsRequest Request) : IRequest<Result>;
