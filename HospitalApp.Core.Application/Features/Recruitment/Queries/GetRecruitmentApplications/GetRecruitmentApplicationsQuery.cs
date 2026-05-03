using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Recruitment.DTOs;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Recruitment.Queries.GetRecruitmentApplications;

public record GetRecruitmentApplicationsQuery(
    RecruitmentStageEnum? StageFilter,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedResult<RecruitmentApplicationDto>>>;
