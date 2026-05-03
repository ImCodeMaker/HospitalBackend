using AutoMapper;
using HospitalApp.Core.Application.Features.InsuranceCompanies.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class InsuranceCompanyMappingProfile : Profile
{
    public InsuranceCompanyMappingProfile()
    {
        CreateMap<InsuranceCompany, InsuranceCompanyDto>();
    }
}
