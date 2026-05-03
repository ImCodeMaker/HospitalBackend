using AutoMapper;
using HospitalApp.Core.Application.Features.Prescriptions.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class PrescriptionMappingProfile : Profile
{
    public PrescriptionMappingProfile()
    {
        CreateMap<MedicalPrescription, PrescriptionDto>();
    }
}
