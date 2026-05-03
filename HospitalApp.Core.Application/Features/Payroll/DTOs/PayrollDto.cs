namespace HospitalApp.Core.Application.Features.Payroll.DTOs;

public class PayrollRecordDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public DateOnly PayPeriodStart { get; init; }
    public DateOnly PayPeriodEnd { get; init; }
    public decimal GrossPay { get; init; }
    public decimal AfpEmployee { get; init; }
    public decimal AfpEmployer { get; init; }
    public decimal ArsEmployee { get; init; }
    public decimal ArsEmployer { get; init; }
    public decimal IsrWithholding { get; init; }
    public decimal NetPay { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record CreatePayrollRecordRequest(
    Guid EmployeeId,
    DateOnly PayPeriodStart,
    DateOnly PayPeriodEnd,
    decimal GrossPay,
    decimal AfpEmployee,
    decimal AfpEmployer,
    decimal ArsEmployee,
    decimal ArsEmployer,
    decimal IsrWithholding);
