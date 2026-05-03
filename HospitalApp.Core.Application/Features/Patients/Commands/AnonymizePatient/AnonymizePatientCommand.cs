using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Commands.AnonymizePatient;

public record AnonymizePatientCommand(Guid PatientId) : IRequest<Result>;
