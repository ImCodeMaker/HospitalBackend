using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Prescriptions.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Prescriptions.Queries.GetPrescriptionsByConsult;

public class GetPrescriptionsByConsultQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetPrescriptionsByConsultQuery, Result<List<PrescriptionDto>>>
{
    public async Task<Result<List<PrescriptionDto>>> Handle(GetPrescriptionsByConsultQuery query, CancellationToken ct)
    {
        var prescriptions = await uow.Prescriptions.FindAsync(p => p.ConsultId == query.ConsultId, ct);
        return Result<List<PrescriptionDto>>.Success(
            mapper.Map<List<PrescriptionDto>>(prescriptions.OrderBy(p => p.CreatedAt).ToList()));
    }
}
