using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HospitalApp.WebAPI.Middleware;

public sealed class IdempotencyOptions
{
    public const string SectionName = "Idempotency";

    public bool Enabled { get; set; } = true;
    public string HeaderName { get; set; } = "Idempotency-Key";
    public int RetentionHours { get; set; } = 24;
    public int MaxKeyLength { get; set; } = 160;
    public int MaxResponseBytes { get; set; } = 1_048_576;
}

public class IdempotencyMiddleware(
    RequestDelegate next,
    IOptions<IdempotencyOptions> options,
    ILogger<IdempotencyMiddleware> logger)
{
    private const string ProcessingStatus = "Processing";
    private const string CompletedStatus = "Completed";

    private static readonly HashSet<string> UnsafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete,
    };

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        var opts = options.Value;
        if (!ShouldApply(context, opts, out var key))
        {
            await next(context);
            return;
        }

        if (key!.Length > opts.MaxKeyLength)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = $"Idempotency-Key must be {opts.MaxKeyLength} characters or fewer."
            }, context.RequestAborted);
            return;
        }

        var requestHash = await ComputeRequestHashAsync(context.Request);
        var method = context.Request.Method.ToUpperInvariant();
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "/";
        var userId = GetUserId(context.User);
        var now = DateTime.UtcNow;

        var existing = await db.IdempotencyRequests
            .SingleOrDefaultAsync(i => i.Key == key
                                       && i.Method == method
                                       && i.Path == path
                                       && i.UserId == userId,
                context.RequestAborted);

        if (existing is not null)
        {
            if (existing.ExpiresAt <= now)
            {
                db.IdempotencyRequests.Remove(existing);
                await db.SaveChangesAsync(context.RequestAborted);
            }
            else if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Idempotency-Key was already used with a different request payload."
                }, context.RequestAborted);
                return;
            }
            else if (string.Equals(existing.Status, CompletedStatus, StringComparison.Ordinal))
            {
                await ReplayAsync(context, existing);
                return;
            }
            else
            {
                context.Response.Headers.RetryAfter = "2";
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "An operation with this Idempotency-Key is still processing. Retry the same request shortly."
                }, context.RequestAborted);
                return;
            }
        }

        var entry = new IdempotencyRequest
        {
            Key = key,
            RequestHash = requestHash,
            Method = method,
            Path = path,
            UserId = userId,
            Status = ProcessingStatus,
            ExpiresAt = now.AddHours(Math.Max(1, opts.RetentionHours)),
        };

        db.IdempotencyRequests.Add(entry);
        try
        {
            await db.SaveChangesAsync(context.RequestAborted);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Concurrent idempotency insert for {Method} {Path}", method, path);
            context.Response.Headers.RetryAfter = "2";
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An operation with this Idempotency-Key is already being processed. Retry the same request shortly."
            }, context.RequestAborted);
            return;
        }

        await CaptureAndStoreResponseAsync(context, db, entry, opts);
    }

    private static bool ShouldApply(HttpContext context, IdempotencyOptions opts, out string? key)
    {
        key = null;
        if (!opts.Enabled ||
            !UnsafeMethods.Contains(context.Request.Method) ||
            context.User.Identity?.IsAuthenticated != true ||
            context.Request.HasFormContentType)
            return false;

        key = context.Request.Headers[opts.HeaderName].FirstOrDefault()?.Trim();
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return true;
    }

    private async Task CaptureAndStoreResponseAsync(
        HttpContext context,
        ApplicationDbContext db,
        IdempotencyRequest entry,
        IdempotencyOptions opts)
    {
        var originalBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            await next(context);
            responseBuffer.Position = 0;

            if (context.Response.StatusCode < 500 && responseBuffer.Length <= opts.MaxResponseBytes)
            {
                using var reader = new StreamReader(responseBuffer, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync(context.RequestAborted);
                entry.Status = CompletedStatus;
                entry.ResponseStatusCode = context.Response.StatusCode;
                entry.ResponseContentType = context.Response.ContentType;
                entry.ResponseLocation = context.Response.Headers.Location.FirstOrDefault();
                entry.ResponseBody = body;
                entry.CompletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(context.RequestAborted);
            }
        }
        finally
        {
            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalBody, context.RequestAborted);
            context.Response.Body = originalBody;
        }
    }

    private static async Task ReplayAsync(HttpContext context, IdempotencyRequest existing)
    {
        context.Response.StatusCode = existing.ResponseStatusCode ?? StatusCodes.Status200OK;
        context.Response.ContentType = existing.ResponseContentType ?? "application/json";
        context.Response.Headers["Idempotency-Replayed"] = "true";
        if (!string.IsNullOrWhiteSpace(existing.ResponseLocation))
            context.Response.Headers.Location = existing.ResponseLocation;

        await context.Response.WriteAsync(existing.ResponseBody ?? string.Empty, context.RequestAborted);
    }

    private static async Task<string> ComputeRequestHashAsync(HttpRequest request)
    {
        request.EnableBuffering();
        request.Body.Position = 0;
        using var sha = SHA256.Create();
        await sha.ComputeHashAsync(request.Body, request.HttpContext.RequestAborted);
        request.Body.Position = 0;

        var route = $"{request.Method}:{request.Path.Value?.ToLowerInvariant()}:{request.QueryString.Value}";
        var routeBytes = Encoding.UTF8.GetBytes(route);
        var bodyHash = sha.Hash ?? [];
        return Convert.ToHexString(SHA256.HashData(routeBytes.Concat(bodyHash).ToArray())).ToLowerInvariant();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
