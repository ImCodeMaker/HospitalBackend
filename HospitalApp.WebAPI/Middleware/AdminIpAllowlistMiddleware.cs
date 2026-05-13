using System.Net;
using Microsoft.Extensions.Options;

namespace HospitalApp.WebAPI.Middleware;

public class AdminIpAllowlistOptions
{
    public bool Enabled { get; set; }
    public string[] AllowedCidrs { get; set; } = [];
    /// <summary>Path prefixes (lowercased) that require IP allowlisting. Example: "/api/v1/audit".</summary>
    public string[] ProtectedPaths { get; set; } = [];
}

public class AdminIpAllowlistMiddleware(RequestDelegate next, IOptions<AdminIpAllowlistOptions> options, ILogger<AdminIpAllowlistMiddleware> logger)
{
    private readonly AdminIpAllowlistOptions _opts = options.Value;

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (!_opts.Enabled || _opts.ProtectedPaths.Length == 0)
        {
            await next(ctx);
            return;
        }

        var path = ctx.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (!_opts.ProtectedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(ctx);
            return;
        }

        var ip = ctx.Connection.RemoteIpAddress;
        if (ip is null)
        {
            ctx.Response.StatusCode = 403;
            return;
        }

        if (!_opts.AllowedCidrs.Any(c => IsInCidr(ip, c)))
        {
            logger.LogWarning("Blocked admin path {Path} from non-allowlisted IP {Ip}", path, ip);
            ctx.Response.StatusCode = 403;
            await ctx.Response.WriteAsync("Forbidden — IP not on allowlist.");
            return;
        }

        await next(ctx);
    }

    private static bool IsInCidr(IPAddress ip, string cidr)
    {
        if (string.IsNullOrEmpty(cidr)) return false;
        var parts = cidr.Split('/');
        if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var network) || !int.TryParse(parts[1], out var prefix))
            return false;

        var ipBytes = ip.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();
        if (ipBytes.Length != networkBytes.Length) return false;

        var byteCount = prefix / 8;
        var bitsRemaining = prefix % 8;

        for (var i = 0; i < byteCount; i++)
            if (ipBytes[i] != networkBytes[i]) return false;

        if (bitsRemaining == 0) return true;

        var mask = (byte)(0xFF << (8 - bitsRemaining));
        return (ipBytes[byteCount] & mask) == (networkBytes[byteCount] & mask);
    }
}
