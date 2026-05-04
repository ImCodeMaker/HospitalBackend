using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.PatientPortal.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalConsults;

public record GetPortalConsultsQuery(Guid PatientId) : IRequest<Result<List<PortalConsultSummaryDto>>>;
