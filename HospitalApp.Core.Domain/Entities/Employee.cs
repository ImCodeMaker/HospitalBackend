using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class Employee : SharedEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string NationalId { get; set; }
    public required EmployeeRoleEnum Role { get; set; }
    public Guid? SpecialtyId { get; set; }
    public string? MedicalLicenseNumber { get; set; }
    public required DateTime StartDate { get; set; }
    public required EmploymentTypeEnum EmploymentType { get; set; }
    public string? Department { get; set; }
    public Guid? DirectSupervisorId { get; set; }
    public decimal Salary { get; set; }
    public PayFrequencyEnum PayFrequency { get; set; } = PayFrequencyEnum.Monthly;
    public string? BankAccount { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public EmployeeStatusEnum Status { get; set; } = EmployeeStatusEnum.Active;
    public Guid? UserId { get; set; }

    public Specialty? Specialty { get; set; }
    public Employee? DirectSupervisor { get; set; }
    public ICollection<PayrollRecord> PayrollRecords { get; set; } = [];
    public ICollection<RecruitmentApplication> RecruitmentApplications { get; set; } = [];
}
