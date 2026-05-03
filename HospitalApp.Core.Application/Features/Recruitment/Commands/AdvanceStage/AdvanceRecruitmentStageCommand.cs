using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Recruitment.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Recruitment.Commands.AdvanceStage;

public record AdvanceRecruitmentStageCommand(
    Guid ApplicationId,
    AdvanceRecruitmentStageRequest Request) : IRequest<Result>;
