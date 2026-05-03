using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Patients.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Queries.GetPatientById;

public record GetPatientByIdQuery(Guid PatientId) : IRequest<Result<PatientDto>>;
