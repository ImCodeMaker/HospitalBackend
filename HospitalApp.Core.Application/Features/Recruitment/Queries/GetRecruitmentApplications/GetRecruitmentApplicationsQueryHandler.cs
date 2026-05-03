using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Recruitment.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Recruitment.Queries.GetRecruitmentApplications;

public class GetRecruitmentApplicationsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetRecruitmentApplicationsQuery, Result<PaginatedResult<RecruitmentApplicationDto>>>
{
    public async Task<Result<PaginatedResult<RecruitmentApplicationDto>>> Handle(
        GetRecruitmentApplicationsQuery query, CancellationToken ct)
    {
        var all = query.StageFilter.HasValue
            ? await uow.RecruitmentApplications.FindAsync(a => a.Stage == query.StageFilter.Value, ct)
            : await uow.RecruitmentApplications.GetAllAsync(ct);

        var ordered = all.OrderByDescending(a => a.AppliedDate).ToList();
        var paged = ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var dtos = paged.Select(a => new RecruitmentApplicationDto
        {
            Id = a.Id,
            ApplicantName = a.ApplicantName,
            Position = a.Position,
            Stage = a.Stage.ToString(),
            AppliedDate = a.AppliedDate,
            InterviewDate = a.InterviewDate,
            Notes = a.InterviewNotes,
            CvFilePath = a.CvFilePath,
            CreatedAt = a.CreatedAt
        }).ToList();

        return Result<PaginatedResult<RecruitmentApplicationDto>>.Success(
            PaginatedResult<RecruitmentApplicationDto>.Create(dtos, ordered.Count, query.Page, query.PageSize));
    }
}
