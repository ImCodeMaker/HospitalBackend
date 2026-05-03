using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Queries.CheckDuplicate;

public class CheckDuplicatePatientQueryHandler(IUnitOfWork uow)
    : IRequestHandler<CheckDuplicatePatientQuery, Result<DuplicatePatientResult>>
{
    public async Task<Result<DuplicatePatientResult>> Handle(CheckDuplicatePatientQuery query, CancellationToken ct)
    {
        var existing = await uow.Patients.FirstOrDefaultAsync(
            p => p.DocumentType == query.DocumentType &&
                 p.DocumentNumber == query.DocumentNumber, ct);

        if (existing is null)
            return Result<DuplicatePatientResult>.Success(new DuplicatePatientResult(false, null, null));

        return Result<DuplicatePatientResult>.Success(
            new DuplicatePatientResult(true, existing.Id, $"{existing.FirstName} {existing.LastName}"));
    }
}
