namespace HospitalApp.Core.Application.Features.InsuranceCompanies.DTOs;

public class InsuranceCompanyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ContactPhone { get; init; }
    public string? ContactEmail { get; init; }
    public decimal DefaultCoveragePercentage { get; init; }
    public bool IsActive { get; init; }
}
