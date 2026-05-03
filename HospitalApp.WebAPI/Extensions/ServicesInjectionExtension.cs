using System.Text;
using System.Threading.RateLimiting;
using HospitalApp.Core.Application.Common;
using HospitalApp.Infrastructure.Identity.Settings;
using HospitalApp.Infrastructure.Shared.Services;
using HospitalApp.Infrastructure.Shared.Settings;
using HospitalApp.WebAPI.Extensions.Swagger;
using HospitalApp.WebAPI.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;

namespace HospitalApp.WebAPI.Extensions;

public static class ServicesInjectionExtension
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(opts => opts.AddPolicy("CorsPolicy", policy =>
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()));

        services.Configure<BusinessInfo>(configuration.GetSection(nameof(BusinessInfo)));
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IDashboardNotifier, DashboardNotifier>();
        services.AddScoped<IPdfService, PdfService>();
        services.Configure<SmtpSettings>(configuration.GetSection(nameof(SmtpSettings)));
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.Configure<TwilioSettings>(configuration.GetSection(nameof(TwilioSettings)));
        services.AddScoped<ISmsService, TwilioSmsService>();
        services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 52_428_800);
        services.AddSignalR();

        var redisConn = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConn))
        {
            services.AddStackExchangeRedisCache(opts =>
            {
                opts.Configuration = redisConn;
                opts.InstanceName = "lovasalud:";
            });
        }
        else
        {
            services.AddDistributedMemoryCache(); // fallback for dev without Redis
        }

        services.AddRateLimiter(opts =>
        {
            opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    }));
            opts.OnRejected = async (ctx, ct) =>
            {
                ctx.HttpContext.Response.StatusCode = 429;
                await ctx.HttpContext.Response.WriteAsync("Too many requests.", ct);
            };
        });
        // Use NameIdentifier claim (sub = userId Guid) as SignalR user identity
        services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider,
            Microsoft.AspNetCore.SignalR.DefaultUserIdProvider>();
        services.AddSwagger();

        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>()!;
        services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero
                };
                // SignalR sends token via query string over WebSocket
                opts.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) &&
                            ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                            ctx.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", p => p.RequireRole("Admin"))
            .AddPolicy("ClinicalStaff", p => p.RequireRole("Admin", "Doctor", "Receptionist", "LabTechnician", "Nurse"))
            .AddPolicy("DoctorOrAdmin", p => p.RequireRole("Admin", "Doctor"));

        services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }
}