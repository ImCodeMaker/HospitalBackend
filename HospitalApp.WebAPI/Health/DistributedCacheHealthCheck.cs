using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HospitalApp.WebAPI.Health;

public class DistributedCacheHealthCheck(IDistributedCache cache) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"health:{Guid.NewGuid():N}";
            await cache.SetStringAsync(
                key,
                "ok",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                },
                cancellationToken);
            var value = await cache.GetStringAsync(key, cancellationToken);

            return value == "ok"
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Distributed cache read/write check failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Distributed cache is unavailable.", ex);
        }
    }
}
