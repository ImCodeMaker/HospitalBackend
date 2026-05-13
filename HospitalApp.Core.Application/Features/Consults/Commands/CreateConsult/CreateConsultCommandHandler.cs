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
        var req = command.Request;
        Patient? patient;
        var quickRegistered = false;

        if (req.PatientId.HasValue)
        {
            patient = await uow.Patients.GetByIdAsync(req.PatientId.Value, ct);
            if (patient is null)
                return Result<Guid>.NotFound("Patient not found.");
        }
        else if (req.QuickPatient is not null)
        {
            var qp = req.QuickPatient;
            // Duplicate check by document number
            var dup = (await uow.Patients.FindAsync(
                p => p.DocumentType == qp.DocumentType && p.DocumentNumber == qp.DocumentNumber, ct))
                .FirstOrDefault();

            if (dup is not null)
            {
                patient = dup;
            }
            else
            {
                patient = new Patient
                {
                    FirstName = qp.FirstName,
                    LastName = qp.LastName,
                    DocumentType = qp.DocumentType,
                    DocumentNumber = qp.DocumentNumber,
                    Nationality = qp.Nationality ?? "Dominicana",
                    HomeAddress = qp.Address ?? "—",
                    BirthDate = qp.BirthDate,
                    Gender = qp.Gender,
                    Status = PatientsStatus.PendingVerification,
                    Email = qp.Email,
                    Phone = qp.Phone,
                };
                await uow.Patients.AddAsync(patient, ct);
                quickRegistered = true;
            }
        }
        else
        {
            return Result<Guid>.Failure("PatientId or QuickPatient is required.", 400);
        }

        // Alert: check for active pending consult
        var hasPending = await uow.Consults.ExistsAsync(
            c => c.PatientId == patient.Id
              && (c.Status == ConsultStatusEnum.Open || c.Status == ConsultStatusEnum.InProgress), ct);

        var bmi = (req.WeightKg.HasValue && req.HeightCm.HasValue && req.HeightCm > 0)
            ? Math.Round(req.WeightKg.Value / (decimal)Math.Pow((double)(req.HeightCm.Value / 100), 2), 1)
            : (decimal?)null;

        var consult = new Consult
        {
            PatientId = patient.Id,
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

        var warnings = new List<string>();
        if (quickRegistered)
            warnings.Add("Patient was created inline as PendingVerification. Complete the demographics profile to verify.");
        if (hasPending)
            warnings.Add("Patient already has an active consult (Open or InProgress). A new consult was created anyway.");

        return Result<Guid>.Created(consult.Id, warnings.Count > 0 ? string.Join(" ", warnings) : null);
    }
}
