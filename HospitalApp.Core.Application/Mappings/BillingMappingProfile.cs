using AutoMapper;
using HospitalApp.Core.Application.Features.Billing.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class BillingMappingProfile : Profile
{
    public BillingMappingProfile()
    {
        CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.PatientName, o => o.MapFrom(s =>
                s.Patient != null ? $"{s.Patient.FirstName} {s.Patient.LastName}" : string.Empty))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.NcfType, o => o.MapFrom(s => s.NcfType != null ? s.NcfType.ToString() : null))
            .ForMember(d => d.BalanceDue, o => o.MapFrom(s => s.BalanceDue));

        CreateMap<InvoiceLineItem, InvoiceLineItemDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.PatientAmount, o => o.MapFrom(s => s.PatientAmount));

        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Method, o => o.MapFrom(s => s.Method.ToString()));
    }
}
