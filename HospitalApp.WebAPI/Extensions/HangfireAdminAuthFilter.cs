using System.Net.Http.Headers;
using System.Text;
using Hangfire.Dashboard;

namespace HospitalApp.WebAPI.Extensions;

public class HangfireBasicAuthFilter(string user, string password) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();

        var header = http.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(http);
            return false;
        }

        try
        {
            var encoded = header["Basic ".Length..].Trim();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var parts = decoded.Split(':', 2);
            if (parts.Length == 2 && parts[0] == user && parts[1] == password)
                return true;
        }
        catch { /* malformed header */ }

        Challenge(http);
        return false;
    }

    private static void Challenge(HttpContext http)
    {
        http.Response.StatusCode = 401;
        http.Response.Headers.WWWAuthenticate = "Basic realm=\"Hangfire Dashboard\"";
    }
}
