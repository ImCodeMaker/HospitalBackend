using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Patients.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Commands.CreatePatient;

public record CreatePatientCommand(CreatePatientRequest Request) : IRequest<Result<Guid>>;
