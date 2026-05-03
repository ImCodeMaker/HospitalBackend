using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Queries.GetConsultById;

public class GetConsultByIdQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetConsultByIdQuery, Result<ConsultDto>>
{
    public async Task<Result<ConsultDto>> Handle(GetConsultByIdQuery query, CancellationToken ct)
    {
        var consult = await uow.Consults.GetByIdAsync(query.ConsultId, ct);
        if (consult is null)
            return Result<ConsultDto>.NotFound("Consult not found.");

        return Result<ConsultDto>.Success(mapper.Map<ConsultDto>(consult));
    }
}
