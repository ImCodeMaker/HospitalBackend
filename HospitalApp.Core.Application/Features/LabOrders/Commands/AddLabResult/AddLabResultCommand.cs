using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Commands.AddLabResult;

public record AddLabResultRequest(
    string TestName,
    string? Value,
    string? Unit,
    string? ReferenceRange,
    LabResultFlagEnum Flag,
    string? Notes,
    string? ResultFilePath
);

public record AddLabResultCommand(Guid LabOrderId, AddLabResultRequest Request, Guid EnteredByUserId)
    : IRequest<Result<Guid>>;
