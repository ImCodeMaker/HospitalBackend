using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Commands.CreateConsult;

public class CreateConsultCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateConsultCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateConsultCommand command, CancellationToken ct)
    {
        var patient = await uow.Patients.GetByIdAsync(command.Request.PatientId, ct);
        if (patient is null)
            return Result<Guid>.NotFound("Patient not found.");

        // Alert: check for active pending consult
        var hasPending = await uow.Consults.ExistsAsync(
            c => c.PatientId == command.Request.PatientId
              && (c.Status == ConsultStatusEnum.Open || c.Status == ConsultStatusEnum.InProgress), ct);

        var req = command.Request;
        var bmi = (req.WeightKg.HasValue && req.HeightCm.HasValue && req.HeightCm > 0)
            ? Math.Round(req.WeightKg.Value / (decimal)Math.Pow((double)(req.HeightCm.Value / 100), 2), 1)
            : (decimal?)null;

        var consult = new Consult
        {
            PatientId = req.PatientId,
            SpecialtyId = req.SpecialtyId,
            DoctorId = command.DoctorId,
            Status = ConsultStatusEnum.Open,
            WeightKg = req.WeightKg,
            HeightCm = req.HeightCm,
            Bmi = bmi,
            BpSystolic = req.BpSystolic,
            BpDiastolic = req.BpDiastolic,
            HeartRate = req.HeartRate,
            TemperatureCelsius = req.TemperatureCelsius,
            O2Saturation = req.O2Saturation,
            RespiratoryRate = req.RespiratoryRate,
            ChiefComplaint = req.ChiefComplaint,
            StartedAt = DateTime.UtcNow,
        };

        await uow.Consults.AddAsync(consult, ct);
        await uow.SaveChangesAsync(ct);

        var warning = hasPending
            ? "Patient already has an active consult (Open or InProgress). A new consult was created anyway."
            : null;

        return Result<Guid>.Created(consult.Id, warning);
    }
}
