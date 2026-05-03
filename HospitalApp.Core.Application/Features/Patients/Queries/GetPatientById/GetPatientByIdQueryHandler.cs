using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Patients.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Queries.GetPatientById;

public class GetPatientByIdQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
{
    public async Task<Result<PatientDto>> Handle(GetPatientByIdQuery query, CancellationToken ct)
    {
        var patient = await uow.Patients.GetByIdAsync(query.PatientId, ct);
        if (patient is null)
            return Result<PatientDto>.NotFound($"Patient {query.PatientId} not found.");

        return Result<PatientDto>.Success(mapper.Map<PatientDto>(patient));
    }
}
