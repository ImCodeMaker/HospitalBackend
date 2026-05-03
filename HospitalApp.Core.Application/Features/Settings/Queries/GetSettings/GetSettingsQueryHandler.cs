using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Settings.DTOs;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Settings.Queries.GetSettings;

public class GetSettingsQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetSettingsQuery, Result<ClinicSettingsDto>>
{
    public async Task<Result<ClinicSettingsDto>> Handle(GetSettingsQuery query, CancellationToken ct)
    {
        var settings = await uow.ClinicSettings.FirstOrDefaultAsync(_ => true, ct);
        if (settings is null)
        {
            // Return defaults if not configured yet
            settings = new ClinicSettings { ClinicName = "Lova Salud" };
        }
        return Result<ClinicSettingsDto>.Success(mapper.Map<ClinicSettingsDto>(settings));
    }
}
