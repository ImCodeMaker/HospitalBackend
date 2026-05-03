using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.LabOrders.Commands.AddLabResult;

public class AddLabResultCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier, IEmailService email)
    : IRequestHandler<AddLabResultCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddLabResultCommand command, CancellationToken ct)
    {
        var order = await uow.LabOrders.GetByIdAsync(command.LabOrderId, ct);
        if (order is null)
            return Result<Guid>.NotFound("Lab order not found.");

        var req = command.Request;
        var result = new LabResult
        {
            LabOrderId = order.Id,
            EnteredByUserId = command.EnteredByUserId,
            TestName = req.TestName,
            Value = req.Value,
            Unit = req.Unit,
            ReferenceRange = req.ReferenceRange,
            Flag = req.Flag,
            Notes = req.Notes,
            ResultFilePath = req.ResultFilePath,
        };

        order.Status = "Complete";
        order.ResultsAvailableAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await uow.LabResults.AddAsync(result, ct);
        uow.LabOrders.Update(order);
        await uow.SaveChangesAsync(ct);

        if (req.Flag == LabResultFlagEnum.Critical)
            await notifier.NotifyCriticalLabResultAsync(
                order.OrderedByDoctorId, order.Id, req.TestName, req.Value, ct);

        var patient = await uow.Patients.GetByIdAsync(order.PatientId, ct);
        if (patient is not null && !string.IsNullOrEmpty(patient.Email))
        {
            try
            {
                var htmlBody = $"""
                    <p>Estimado/a <strong>{patient.FirstName} {patient.LastName}</strong>,</p>
                    <p>El resultado de su examen de laboratorio ya está disponible:</p>
                    <ul>
                        <li><strong>Prueba:</strong> {req.TestName}</li>
                        <li><strong>Fecha:</strong> {order.ResultsAvailableAt:dd/MM/yyyy HH:mm}</li>
                    </ul>
                    <p>Por favor comuníquese con su médico para revisar los resultados.</p>
                    <p>Gracias,<br/>Lova Salud</p>
                    """;

                await email.SendAsync(patient.Email, "Resultado de laboratorio disponible", htmlBody, ct);
            }
            catch
            {
                // notification failure must not block operation
            }
        }

        return Result<Guid>.Created(result.Id);
    }
}
