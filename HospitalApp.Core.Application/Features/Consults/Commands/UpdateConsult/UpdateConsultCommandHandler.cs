using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Commands.UpdateConsult;

public class UpdateConsultCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateConsultCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateConsultCommand command, CancellationToken ct)
    {
        var consult = await uow.Consults.GetByIdAsync(command.ConsultId, ct);
        if (consult is null)
            return Result<Guid>.NotFound("Consult not found.");

        if (consult.DoctorId != command.DoctorId)
            return Result<Guid>.Forbidden("You can only update your own consults.");

        if (consult.Status == ConsultStatusEnum.Finished)
            return Result<Guid>.Failure("Cannot edit a finished consult.", 409);

        var req = command.Request;
        if (req.WeightKg.HasValue) consult.WeightKg = req.WeightKg;
        if (req.HeightCm.HasValue) consult.HeightCm = req.HeightCm;
        if (req.BpSystolic.HasValue) consult.BpSystolic = req.BpSystolic;
        if (req.BpDiastolic.HasValue) consult.BpDiastolic = req.BpDiastolic;
        if (req.HeartRate.HasValue) consult.HeartRate = req.HeartRate;
        if (req.TemperatureCelsius.HasValue) consult.TemperatureCelsius = req.TemperatureCelsius;
        if (req.O2Saturation.HasValue) consult.O2Saturation = req.O2Saturation;
        if (req.RespiratoryRate.HasValue) consult.RespiratoryRate = req.RespiratoryRate;
        if (req.ChiefComplaint is not null) consult.ChiefComplaint = req.ChiefComplaint;
        if (req.ClinicalObservations is not null) consult.ClinicalObservations = req.ClinicalObservations;
        if (req.DiagnosisCodes is not null) consult.DiagnosisCodes = req.DiagnosisCodes;
        if (req.DiagnosisDescription is not null) consult.DiagnosisDescription = req.DiagnosisDescription;
        if (req.TreatmentPlan is not null) consult.TreatmentPlan = req.TreatmentPlan;
        if (req.ReferralNotes is not null) consult.ReferralNotes = req.ReferralNotes;
        if (req.SpecialtyData is not null) consult.SpecialtyData = req.SpecialtyData;
        if (req.DentalChart is not null) consult.DentalChart = req.DentalChart;

        if (consult.WeightKg.HasValue && consult.HeightCm.HasValue && consult.HeightCm > 0)
            consult.Bmi = Math.Round(consult.WeightKg.Value / (decimal)Math.Pow((double)(consult.HeightCm.Value / 100), 2), 1);

        consult.Status = ConsultStatusEnum.InProgress;
        consult.UpdatedAt = DateTime.UtcNow;

        uow.Consults.Update(consult);
        await uow.SaveChangesAsync(ct);

        return Result<Guid>.Success(consult.Id);
    }
}
