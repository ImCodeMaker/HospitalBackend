using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Caja.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Caja.Queries.GetCurrentShift;

public class GetCurrentShiftQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetCurrentShiftQuery, Result<CajaShiftDto?>>
{
    public async Task<Result<CajaShiftDto?>> Handle(GetCurrentShiftQuery query, CancellationToken ct)
    {
        var shift = await uow.CajaShifts.FirstOrDefaultAsync(s => s.IsOpen, ct);
        return Result<CajaShiftDto?>.Success(shift is null ? null : mapper.Map<CajaShiftDto>(shift));
    }
}
