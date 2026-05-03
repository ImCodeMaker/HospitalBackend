using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.InsuranceCompanies.Commands.CreateInsuranceCompany;

public record CreateInsuranceCompanyRequest(string Name, string? ContactPhone, string? ContactEmail, string? ClaimInstructions, decimal DefaultCoveragePercentage);
public record CreateInsuranceCompanyCommand(CreateInsuranceCompanyRequest Request) : IRequest<Result<Guid>>;
