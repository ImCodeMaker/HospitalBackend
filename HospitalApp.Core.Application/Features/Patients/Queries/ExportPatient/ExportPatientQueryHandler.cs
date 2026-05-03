using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Queries.ExportPatient;

public class ExportPatientQueryHandler(IUnitOfWork uow)
    : IRequestHandler<ExportPatientQuery, Result<PatientExportDto>>
{
    public async Task<Result<PatientExportDto>> Handle(ExportPatientQuery query, CancellationToken ct)
    {
        var patient = await uow.Patients.GetByIdAsync(query.PatientId, ct);
        if (patient is null)
            return Result<PatientExportDto>.NotFound($"Patient {query.PatientId} not found.");

        var consults      = await uow.Consults.FindAsync(c => c.PatientId == query.PatientId, ct);
        var invoices      = await uow.Invoices.FindAsync(i => i.PatientId == query.PatientId, ct);
        var appointments  = await uow.Appointments.FindAsync(a => a.PatientId == query.PatientId, ct);

        var dto = new PatientExportDto(
            Id:                         patient.Id,
            FirstName:                  patient.FirstName,
            LastName:                   patient.LastName,
            DocumentType:               patient.DocumentType.ToString(),
            DocumentNumber:             patient.DocumentNumber,
            Nationality:                patient.Nationality,
            HomeAddress:                patient.HomeAddress,
            BirthDate:                  patient.BirthDate,
            Gender:                     patient.Gender.ToString(),
            Status:                     patient.Status.ToString(),
            Email:                      patient.Email,
            Phone:                      patient.Phone,
            BloodType:                  patient.BloodType.ToString(),
            KnownAllergies:             patient.KnownAllergies,
            ChronicConditions:          patient.ChronicConditions,
            IsMinor:                    patient.IsMinor,
            GuardianFirstName:          patient.GuardianFirstName,
            GuardianLastName:           patient.GuardianLastName,
            GuardianDocumentType:       patient.GuardianDocumentType?.ToString(),
            GuardianDocumentNumber:     patient.GuardianDocumentNumber,
            GuardianRelationship:       patient.GuardianRelationship?.ToString(),
            GuardianPhone:              patient.GuardianPhone,
            GuardianEmail:              patient.GuardianEmail,
            HasInsurance:               patient.HasInsurance,
            InsurancePolicyNumber:      patient.InsurancePolicyNumber,
            InsurancePolicyHolderName:  patient.InsurancePolicyHolderName,
            InsuranceCoveragePercentage: patient.InsuranceCoveragePercentage,
            CreatedAt:                  patient.CreatedAt,
            UpdatedAt:                  patient.UpdatedAt == default ? null : patient.UpdatedAt,
            Consults: consults.Select(c => new ConsultSummary(
                ConsultId:            c.Id,
                Date:                 c.CreatedAt,
                Status:               c.Status.ToString(),
                DiagnosisDescription: c.DiagnosisDescription
            )).ToList(),
            Invoices: invoices.Select(i => new InvoiceSummary(
                InvoiceId:    i.Id,
                Date:         i.CreatedAt,
                TotalAmount:  i.TotalAmount,
                Status:       i.Status.ToString()
            )).ToList(),
            Appointments: appointments.Select(a => new AppointmentSummary(
                AppointmentId: a.Id,
                Date:          a.ScheduledDate,
                Type:          a.Type.ToString(),
                Status:        a.Status.ToString()
            )).ToList()
        );

        return Result<PatientExportDto>.Success(dto);
    }
}
