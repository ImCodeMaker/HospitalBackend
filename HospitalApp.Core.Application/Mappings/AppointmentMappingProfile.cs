using AutoMapper;
using HospitalApp.Core.Application.Features.Appointments.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class AppointmentMappingProfile : Profile
{
    public AppointmentMappingProfile()
    {
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s =>
                s.Patient != null ? $"{s.Patient.FirstName} {s.Patient.LastName}" : string.Empty))
            .ForMember(d => d.DoctorName, o => o.Ignore())
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}
