namespace HospitalApp.WebAPI.Middleware;

public class CsrfProtectionMiddleware(RequestDelegate next)
{
    public const string CookieName = "csrf_token";
    public const string HeaderName = "X-CSRF-TOKEN";

    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET",
        "HEAD",
        "OPTIONS",
        "TRACE"
    };

    private static readonly PathString[] ExcludedPaths =
    [
        "/api/v1/auth/login",
        "/api/v1/auth/2fa/login",
        "/api/v1/patientsignup"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        if (SafeMethods.Contains(context.Request.Method) ||
            ExcludedPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        var hasRefreshCookie = context.Request.Cookies.ContainsKey("refresh_token");
        if (!hasRefreshCookie)
        {
            await next(context);
            return;
        }

        var cookieToken = context.Request.Cookies[CookieName];
        var headerToken = context.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrWhiteSpace(cookieToken) ||
            string.IsNullOrWhiteSpace(headerToken) ||
            !string.Equals(cookieToken, headerToken, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid CSRF token." });
            return;
        }

        await next(context);
    }
}
