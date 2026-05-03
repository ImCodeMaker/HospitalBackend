using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.InsuranceCompanies.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanies;

public record GetInsuranceCompaniesQuery(bool ActiveOnly = true) : IRequest<Result<List<InsuranceCompanyDto>>>;
