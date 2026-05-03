using AutoMapper;
using HospitalApp.Core.Application.Features.Settings.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class SettingsMappingProfile : Profile
{
    public SettingsMappingProfile()
    {
        CreateMap<ClinicSettings, ClinicSettingsDto>();
    }
}
