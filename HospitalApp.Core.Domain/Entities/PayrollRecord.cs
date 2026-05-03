using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class PayrollRecord : SharedEntity
{
    public required Guid EmployeeId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public required DateTime PeriodStart { get; set; }
    public required DateTime PeriodEnd { get; set; }

    public decimal BaseSalary { get; set; }
    public decimal Bonuses { get; set; }
    public decimal AfpDeduction { get; set; }
    public decimal ArsDeduction { get; set; }
    public decimal IsrDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal NetPay { get; set; }

    public DateTime? PaymentDate { get; set; }
    public PaymentMethodEnum PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public PayrollStatusEnum Status { get; set; } = PayrollStatusEnum.Draft;

    public Employee? Employee { get; set; }
}
