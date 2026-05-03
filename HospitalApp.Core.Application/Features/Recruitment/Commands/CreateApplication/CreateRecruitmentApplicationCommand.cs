using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Recruitment.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Recruitment.Commands.CreateApplication;

public record CreateRecruitmentApplicationCommand(
    CreateRecruitmentApplicationRequest Request) : IRequest<Result<Guid>>;
