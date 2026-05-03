using AutoMapper;
using HospitalApp.Core.Application.Features.LabOrders.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class LabOrderMappingProfile : Profile
{
    public LabOrderMappingProfile()
    {
        CreateMap<LabOrder, LabOrderDto>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s =>
                s.Patient != null ? $"{s.Patient.FirstName} {s.Patient.LastName}" : string.Empty))
            .ForMember(d => d.DoctorName, o => o.Ignore())
            .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()));

        CreateMap<LabResult, LabResultDto>()
            .ForMember(d => d.Flag, o => o.MapFrom(s => s.Flag.ToString()));
    }
}
