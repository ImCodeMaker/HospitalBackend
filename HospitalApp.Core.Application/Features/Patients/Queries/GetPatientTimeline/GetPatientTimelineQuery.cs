using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Patients.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Queries.GetPatientTimeline;

public record GetPatientTimelineQuery(
    Guid PatientId,
    string? Category,   // filter: Consult|Payment|LabOrder|Appointment|null=all
    DateTime? From,
    DateTime? To
) : IRequest<Result<List<PatientTimelineItemDto>>>;
