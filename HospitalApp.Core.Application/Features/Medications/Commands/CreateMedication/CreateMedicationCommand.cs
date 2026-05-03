using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Medications.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Medications.Commands.CreateMedication;

public record CreateMedicationCommand(CreateMedicationRequest Request, Guid CreatedByUserId) : IRequest<Result<Guid>>;
