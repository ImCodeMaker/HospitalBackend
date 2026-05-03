using HospitalApp.Core.Application.Features.Auth.Services;
using HospitalApp.Core.Application.Features.UserManagement.Services;
using HospitalApp.Core.Domain.Interfaces;
using HospitalApp.Infrastructure.Identity.Entities;
using HospitalApp.Infrastructure.Identity.Services;
using HospitalApp.Infrastructure.Identity.Settings;
using HospitalApp.Infrastructure.Persistence.Context;
using HospitalApp.Infrastructure.Persistence.Interceptors;
using HospitalApp.Infrastructure.Persistence.Repositories;
using HospitalApp.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalApp.Infrastructure.Persistence;

public static class ServiceExtensions
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, opts) =>
            opts.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
            {
                opts.Password.RequireDigit = true;
                opts.Password.RequiredLength = 8;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireUppercase = true;
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                opts.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
