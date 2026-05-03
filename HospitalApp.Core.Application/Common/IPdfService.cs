using HospitalApp.Core.Application.Features.Billing.DTOs;
using HospitalApp.Core.Application.Features.Consults.DTOs;
using HospitalApp.Core.Application.Features.Reports.DTOs;

namespace HospitalApp.Core.Application.Common;

public interface IPdfService
{
    byte[] GenerateInvoice(InvoiceDto invoice, string clinicName, string clinicAddress);
    byte[] GenerateSickNote(ConsultDto consult, string patientName, string doctorName, int daysRest);
    byte[] GeneratePrescription(ConsultDto consult, string patientName, string doctorName);
    byte[] GenerateDailyRevenueReport(DailyRevenueSummaryDto reportData, DateTime date);
    byte[] GenerateAccountsReceivableReport(List<AccountsReceivableDto> reportData, DateTime asOf);
    byte[] GenerateInventoryReport(List<InventoryReportItemDto> reportData, DateTime asOf);
    byte[] GenerateShiftReport(ShiftReportData data);
}

public record ShiftReportData(
    Guid ShiftId,
    DateTime OpenedAt,
    DateTime ClosedAt,
    string OpenedByUserId,
    decimal OpeningBalance,
    decimal ClosingBalance,
    decimal ExpectedBalance,
    decimal Discrepancy,
    string? Notes,
    List<ShiftTransactionLine> Transactions);

public record ShiftTransactionLine(
    string Type,
    decimal Amount,
    string? Description,
    bool IsApproved,
    DateTime CreatedAt);
