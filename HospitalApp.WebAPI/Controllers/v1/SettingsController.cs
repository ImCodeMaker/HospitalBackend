using Asp.Versioning;
using HospitalApp.Core.Application.Features.Settings.Commands.UpdateSettings;
using HospitalApp.Core.Application.Features.Settings.DTOs;
using HospitalApp.Core.Application.Features.Settings.Queries.GetSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class SettingsController(IMediator mediator, IDistributedCache cache) : BaseController
{
    private const string ClinicSettingsCacheKey = "settings:clinic";
    private static readonly DistributedCacheEntryOptions SettingsCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
        SlidingExpiration = TimeSpan.FromMinutes(3),
    };

    /// <summary>Get clinic settings.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var cached = await cache.GetStringAsync(ClinicSettingsCacheKey, ct);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var dto = JsonSerializer.Deserialize<ClinicSettingsDto>(cached);
            if (dto is not null)
                return Ok(dto);
        }

        var result = await mediator.Send(new GetSettingsQuery(), ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        await cache.SetStringAsync(
            ClinicSettingsCacheKey,
            JsonSerializer.Serialize(result.Data),
            SettingsCacheOptions,
            ct);

        return Ok(result.Data);
    }

    /// <summary>Update clinic settings. Admin only.</summary>
    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateSettingsCommand(request), ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        await cache.RemoveAsync(ClinicSettingsCacheKey, ct);
        return NoContent();
    }
}
