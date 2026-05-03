namespace HospitalApp.Core.Application.Features.HR.DTOs;

public class EmployeeDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string NationalId { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid? SpecialtyId { get; init; }
    public string? MedicalLicenseNumber { get; init; }
    public DateTime StartDate { get; init; }
    public string EmploymentType { get; init; } = string.Empty;
    public string? Department { get; init; }
    public decimal Salary { get; init; }
    public string PayFrequency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid? UserId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class EmployeePerformanceDto
{
    public Guid EmployeeId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public int PatientsAttended { get; init; }
    public int PrescriptionsWritten { get; init; }
    public int LabOrdersPlaced { get; init; }
    public decimal RevenueGenerated { get; init; }
}
