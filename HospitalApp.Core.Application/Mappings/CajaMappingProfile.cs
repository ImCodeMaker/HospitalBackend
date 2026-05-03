using AutoMapper;
using HospitalApp.Core.Application.Features.Caja.DTOs;
using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Application.Mappings;

public class CajaMappingProfile : Profile
{
    public CajaMappingProfile()
    {
        CreateMap<CajaShift, CajaShiftDto>()
            .ForMember(d => d.Transactions, o => o.MapFrom(s => s.Transactions));

        CreateMap<CashTransaction, CashTransactionDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));
    }
}
