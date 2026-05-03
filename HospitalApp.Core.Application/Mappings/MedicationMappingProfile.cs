using AutoMapper;
using HospitalApp.Core.Application.Features.Medications.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class MedicationMappingProfile : Profile
{
    public MedicationMappingProfile()
    {
        CreateMap<Medication, MedicationDto>()
            .ForMember(d => d.Presentation, o => o.MapFrom(s => s.Presentation.ToString()));
    }
}
