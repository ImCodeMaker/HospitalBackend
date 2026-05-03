using AutoMapper;
using HospitalApp.Core.Application.Features.Patients.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class PatientMappingProfile : Profile
{
    public PatientMappingProfile()
    {
        CreateMap<Patient, PatientDto>()
            .ForMember(d => d.DocumentType, o => o.MapFrom(s => s.DocumentType.ToString()))
            .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.BloodType, o => o.MapFrom(s => s.BloodType.ToString()))
            .ForMember(d => d.GuardianRelationship, o => o.MapFrom(s => s.GuardianRelationship.HasValue ? s.GuardianRelationship.ToString() : null))
            .ForMember(d => d.InsuranceCompanyName, o => o.MapFrom(s => s.InsuranceCompany != null ? s.InsuranceCompany.Name : null));
    }
}
