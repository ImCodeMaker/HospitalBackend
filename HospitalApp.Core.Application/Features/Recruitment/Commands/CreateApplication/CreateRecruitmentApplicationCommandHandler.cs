using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Recruitment.Commands.CreateApplication;

public class CreateRecruitmentApplicationCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateRecruitmentApplicationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRecruitmentApplicationCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var application = new RecruitmentApplication
        {
            ApplicantName = req.ApplicantName,
            Position = req.Position,
            AppliedDate = DateTime.UtcNow,
            Stage = RecruitmentStageEnum.Applicant,
            InterviewNotes = req.Notes,
            CvFilePath = req.CvFilePath
        };

        await uow.RecruitmentApplications.AddAsync(application, ct);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Created(application.Id);
    }
}
