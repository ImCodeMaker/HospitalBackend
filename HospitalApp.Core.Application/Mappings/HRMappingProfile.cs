using AutoMapper;
using HospitalApp.Core.Application.Features.HR.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class HRMappingProfile : Profile
{
    public HRMappingProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.EmploymentType, o => o.MapFrom(s => s.EmploymentType.ToString()))
            .ForMember(d => d.PayFrequency, o => o.MapFrom(s => s.PayFrequency.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}
