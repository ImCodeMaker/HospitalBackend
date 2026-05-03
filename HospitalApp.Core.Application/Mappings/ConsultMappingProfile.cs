using AutoMapper;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class ConsultMappingProfile : Profile
{
    public ConsultMappingProfile()
    {
        CreateMap<Consult, ConsultDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PatientName, o => o.MapFrom(s => s.Patient != null
                ? $"{s.Patient.FirstName} {s.Patient.LastName}" : string.Empty))
            .ForMember(d => d.SpecialtyName, o => o.MapFrom(s => s.Specialty != null
                ? s.Specialty.Name : string.Empty))
            .ForMember(d => d.DoctorName, o => o.Ignore()); // Populated by Identity layer
    }
}
