using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Consults.Commands.CreateConsult;

public class CreateConsultCommandHandler(IUnitOfWork uow, IDashboardNotifier notifier, INcfService ncf)
    : IRequestHandler<CreateConsultCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateConsultCommand command, CancellationToken ct)
    {
        var req = command.Request;
        if (req.Payment is null)
            return Result<Guid>.Failure("Payment is required before opening a consult.", 400);
        if (req.Payment.Amount <= 0)
            return Result<Guid>.Failure("Payment amount must be greater than zero before opening a consult.", 400);
        var transactionType = MapToCashTransactionType(req.Payment.Method);
        if (transactionType is null)
            return Result<Guid>.Failure("Insurance or mixed payments are not supported for prepaid consult opening. Use cash, card, or bank transfer.", 400);

        var shift = await uow.CajaShifts.FirstOrDefaultAsync(s => s.IsOpen, ct);
        if (shift is null)
            return Result<Guid>.Failure("No open caja shift. Open a caja shift before charging and opening a consult.", 409);

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

        var ncfType = req.Payment.NcfType ?? NcfTypeEnum.Consumo;
        var ncfNumber = await ncf.ReserveNextAsync(ncfType, ct);
        if (ncfNumber is null)
            return Result<Guid>.Failure(
                $"NCF range for type {ncfType.GetPrefix()} is exhausted or expired. Configure a new range before opening paid consults.",
                409);

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

        var invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var invoice = new Invoice
        {
            PatientId = patient.Id,
            ConsultId = consult.Id,
            CreatedByUserId = command.DoctorId,
            InvoiceNumber = invoiceNumber,
            Ncf = ncfNumber,
            NcfType = ncfType,
            Status = InvoiceStatusEnum.Paid,
            Subtotal = req.Payment.Amount,
            DiscountAmount = 0,
            TaxAmount = 0,
            InsuranceCoverageAmount = 0,
            TotalAmount = req.Payment.Amount,
            PatientResponsibilityAmount = req.Payment.Amount,
            PaidAmount = req.Payment.Amount,
            PaidAt = DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(req.Payment.Notes)
                ? "Prepaid consult opened after payment."
                : req.Payment.Notes,
        };

        invoice.LineItems.Add(new InvoiceLineItem
        {
            InvoiceId = invoice.Id,
            Type = InvoiceLineItemTypeEnum.ConsultationFee,
            Description = "Consulta medica",
            UnitPrice = req.Payment.Amount,
            Quantity = 1,
        });

        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            ReceivedByUserId = command.DoctorId,
            Amount = req.Payment.Amount,
            Method = req.Payment.Method,
            ReferenceNumber = req.Payment.ReferenceNumber,
            Notes = req.Payment.Notes,
            PaymentDate = DateTime.UtcNow,
        };

        var cashTransaction = new CashTransaction
        {
            ShiftId = shift.Id,
            CreatedByUserId = command.DoctorId,
            InvoiceId = invoice.Id,
            Type = transactionType.Value,
            Amount = req.Payment.Amount,
            Description = $"Prepaid consult {invoice.InvoiceNumber}",
            IsApproved = true,
        };

        await uow.Consults.AddAsync(consult, ct);
        await uow.Invoices.AddAsync(invoice, ct);
        await uow.Payments.AddAsync(payment, ct);
        await uow.CashTransactions.AddAsync(cashTransaction, ct);
        await uow.SaveChangesAsync(ct);
        await notifier.NotifyBillingChangedAsync(ct);
        await notifier.NotifyCajaChangedAsync(ct);

        var warnings = new List<string>();
        if (quickRegistered)
            warnings.Add("Patient was created inline as PendingVerification. Complete the demographics profile to verify.");
        if (hasPending)
            warnings.Add("Patient already has an active consult (Open or InProgress). A new consult was created anyway.");

        return Result<Guid>.Created(consult.Id, warnings.Count > 0 ? string.Join(" ", warnings) : null);
    }

    private static CashTransactionTypeEnum? MapToCashTransactionType(PaymentMethodEnum method) =>
        method switch
        {
            PaymentMethodEnum.Cash => CashTransactionTypeEnum.PaymentCash,
            PaymentMethodEnum.CreditCard or PaymentMethodEnum.DebitCard => CashTransactionTypeEnum.PaymentCard,
            PaymentMethodEnum.BankTransfer => CashTransactionTypeEnum.BankTransfer,
            _ => null,
        };
}
