namespace HospitalApp.WebAPI.Extensions;

/// <summary>
/// 
/// </summary>
public static class CorsExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public static void AddCorsExtension(this IServiceCollection services)
    {
        
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy.WithOrigins("http://localhost:5173");
                policy.AllowCredentials();
                policy.AllowAnyMethod();
                policy.AllowAnyHeader();
            });
        });
    }
}