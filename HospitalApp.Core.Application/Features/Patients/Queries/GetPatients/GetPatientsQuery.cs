using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Patients.DTOs;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Queries.GetPatients;

public record GetPatientsQuery(
    string? SearchTerm,
    PatientsStatus? Status,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedResult<PatientDto>>>;
