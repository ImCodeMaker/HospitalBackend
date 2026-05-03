using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.InsuranceCompanies.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HospitalApp.Core.Application.Features.InsuranceCompanies.Queries.GetInsuranceCompanies;

public class GetInsuranceCompaniesQueryHandler(IUnitOfWork uow, IMapper mapper, IDistributedCache cache)
    : IRequestHandler<GetInsuranceCompaniesQuery, Result<List<InsuranceCompanyDto>>>
{
    private const string CacheKey = "insurance_companies";

    public async Task<Result<List<InsuranceCompanyDto>>> Handle(GetInsuranceCompaniesQuery query, CancellationToken ct)
    {
        var cached = await cache.GetStringAsync(CacheKey, ct);
        if (!string.IsNullOrEmpty(cached))
        {
            var deserialized = JsonSerializer.Deserialize<List<InsuranceCompanyDto>>(cached);
            if (deserialized is not null)
                return Result<List<InsuranceCompanyDto>>.Success(deserialized);
        }

        var all = await uow.InsuranceCompanies.FindAsync(
            ic => !query.ActiveOnly || ic.IsActive, ct);
        var result = mapper.Map<List<InsuranceCompanyDto>>(all.OrderBy(ic => ic.Name).ToList());

        await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(10) }, ct);

        return Result<List<InsuranceCompanyDto>>.Success(result);
    }
}
