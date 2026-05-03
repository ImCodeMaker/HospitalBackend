using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Settings.Commands.UpdateSettings;

public class UpdateSettingsCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateSettingsCommand, Result>
{
    public async Task<Result> Handle(UpdateSettingsCommand command, CancellationToken ct)
    {
        var req = command.Request;
        var settings = await uow.ClinicSettings.FirstOrDefaultAsync(_ => true, ct);

        if (settings is null)
        {
            settings = new ClinicSettings { ClinicName = req.ClinicName };
            await uow.ClinicSettings.AddAsync(settings, ct);
        }

        settings.ClinicName = req.ClinicName;
        settings.Rnc = req.Rnc;
        settings.Address = req.Address;
        settings.Phone = req.Phone;
        settings.Email = req.Email;
        settings.TimeZone = req.TimeZone;
        settings.Currency = req.Currency;
        settings.ItbisRate = req.ItbisRate;
        settings.EmailNotificationsEnabled = req.EmailNotificationsEnabled;
        settings.SmsNotificationsEnabled = req.SmsNotificationsEnabled;
        settings.SessionTimeoutMinutes = req.SessionTimeoutMinutes;

        if (settings.Id != Guid.Empty)
            uow.ClinicSettings.Update(settings);

        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
