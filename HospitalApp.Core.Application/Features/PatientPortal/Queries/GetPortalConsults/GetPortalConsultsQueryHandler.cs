using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.PatientPortal.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalConsults;

public class GetPortalConsultsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetPortalConsultsQuery, Result<List<PortalConsultSummaryDto>>>
{
    public async Task<Result<List<PortalConsultSummaryDto>>> Handle(GetPortalConsultsQuery query, CancellationToken ct)
    {
        var consults = await uow.Consults.FindAsync(c => c.PatientId == query.PatientId, ct);

        var result = new List<PortalConsultSummaryDto>();
        foreach (var c in consults.OrderByDescending(x => x.CreatedAt))
        {
            var specialty = await uow.Specialties.GetByIdAsync(c.SpecialtyId, ct);
            result.Add(new PortalConsultSummaryDto(
                c.Id,
                c.CreatedAt,
                specialty?.Name ?? string.Empty,
                string.Empty,
                c.Status.ToString(),
                c.ChiefComplaint,
                c.DiagnosisDescription,
                c.TreatmentPlan,
                c.FinishedAt
            ));
        }

        return Result<List<PortalConsultSummaryDto>>.Success(result);
    }
}
