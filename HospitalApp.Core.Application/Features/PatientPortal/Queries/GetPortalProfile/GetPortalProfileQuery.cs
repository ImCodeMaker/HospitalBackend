using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.PatientPortal.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.PatientPortal.Queries.GetPortalProfile;

public record GetPortalProfileQuery(Guid PatientId) : IRequest<Result<PortalProfileDto>>;
