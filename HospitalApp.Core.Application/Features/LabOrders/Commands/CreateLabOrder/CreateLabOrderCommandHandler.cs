using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Commands.CreateLabOrder;

public class CreateLabOrderCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateLabOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateLabOrderCommand command, CancellationToken ct)
    {
        var consult = await uow.Consults.GetByIdAsync(command.Request.ConsultId, ct);
        if (consult is null)
            return Result<Guid>.NotFound("Consult not found.");

        var req = command.Request;
        var order = new LabOrder
        {
            ConsultId = req.ConsultId,
            PatientId = consult.PatientId,
            OrderedByDoctorId = command.DoctorId,
            TestName = req.TestName,
            TestCategory = req.TestCategory,
            Priority = req.Priority,
            ClinicalIndication = req.ClinicalIndication,
            SampleType = req.SampleType,
            IsExternal = req.IsExternal,
            ExternalLabName = req.ExternalLabName,
            Status = "Pending",
        };

        await uow.LabOrders.AddAsync(order, ct);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Created(order.Id);
    }
}
