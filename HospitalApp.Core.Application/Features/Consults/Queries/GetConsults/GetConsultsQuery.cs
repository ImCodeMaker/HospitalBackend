using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Queries.GetConsults;

public record GetConsultsQuery(
    string? Status = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedResult<ConsultDto>>>;
