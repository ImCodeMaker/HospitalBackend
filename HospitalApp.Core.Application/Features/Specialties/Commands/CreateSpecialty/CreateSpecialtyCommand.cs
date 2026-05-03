using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Specialties.Commands.CreateSpecialty;

public record CreateSpecialtyRequest(string Name, string Code, SpecialtyTypeEnum Type, string? Description, int DefaultConsultDurationMinutes);
public record CreateSpecialtyCommand(CreateSpecialtyRequest Request) : IRequest<Result<Guid>>;
