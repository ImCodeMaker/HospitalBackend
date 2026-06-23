using HospitalApp.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HospitalApp.WebAPI.Health;

public class ApplicationDbHealthCheck(ApplicationDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Database connection check failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is unavailable.", ex);
        }
    }
}
