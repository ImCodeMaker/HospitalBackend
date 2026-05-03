using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Commands.CreateLabOrder;

public record CreateLabOrderRequest(
    Guid ConsultId,
    string TestName,
    string? TestCategory,
    LabTestPriorityEnum Priority,
    string? ClinicalIndication,
    string? SampleType,
    bool IsExternal,
    string? ExternalLabName
);

public record CreateLabOrderCommand(CreateLabOrderRequest Request, Guid DoctorId) : IRequest<Result<Guid>>;
