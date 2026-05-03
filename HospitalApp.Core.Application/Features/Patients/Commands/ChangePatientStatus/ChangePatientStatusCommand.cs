using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Commands.ChangePatientStatus;

public record ChangePatientStatusCommand(Guid PatientId, PatientsStatus NewStatus, string? Reason)
    : IRequest<Result<Guid>>;
