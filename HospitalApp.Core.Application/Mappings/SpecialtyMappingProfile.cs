using AutoMapper;
using HospitalApp.Core.Application.Features.Specialties.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class SpecialtyMappingProfile : Profile
{
    public SpecialtyMappingProfile()
    {
        CreateMap<Specialty, SpecialtyDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));
    }
}
