using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Specialties.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HospitalApp.Core.Application.Features.Specialties.Queries.GetSpecialties;

public class GetSpecialtiesQueryHandler(IUnitOfWork uow, IMapper mapper, IDistributedCache cache)
    : IRequestHandler<GetSpecialtiesQuery, Result<List<SpecialtyDto>>>
{
    private const string CacheKey = "specialties";

    public async Task<Result<List<SpecialtyDto>>> Handle(GetSpecialtiesQuery query, CancellationToken ct)
    {
        var cached = await cache.GetStringAsync(CacheKey, ct);
        if (!string.IsNullOrEmpty(cached))
        {
            var deserialized = JsonSerializer.Deserialize<List<SpecialtyDto>>(cached);
            if (deserialized is not null)
                return Result<List<SpecialtyDto>>.Success(deserialized);
        }

        var all = await uow.Specialties.GetAllAsync(ct);
        var result = mapper.Map<List<SpecialtyDto>>(all.OrderBy(s => s.Name).ToList());

        await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(10) }, ct);

        return Result<List<SpecialtyDto>>.Success(result);
    }
}
