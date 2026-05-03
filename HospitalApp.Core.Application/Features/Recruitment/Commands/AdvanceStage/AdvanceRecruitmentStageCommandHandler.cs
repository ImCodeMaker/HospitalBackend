using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Recruitment.Commands.AdvanceStage;

public class AdvanceRecruitmentStageCommandHandler(IUnitOfWork uow)
    : IRequestHandler<AdvanceRecruitmentStageCommand, Result>
{
    public async Task<Result> Handle(AdvanceRecruitmentStageCommand command, CancellationToken ct)
    {
        var application = await uow.RecruitmentApplications.GetByIdAsync(command.ApplicationId, ct);
        if (application is null)
            return Result.NotFound("Recruitment application not found.");

        var req = command.Request;

        application.Stage = req.NewStage;

        if (req.InterviewDate.HasValue)
            application.InterviewDate = req.InterviewDate;

        if (req.Notes is not null)
            application.InterviewNotes = req.Notes;

        application.UpdatedAt = DateTime.UtcNow;

        uow.RecruitmentApplications.Update(application);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
