using HospitalApp.Core.Application;
using HospitalApp.Infrastructure.Identity.Entities;
using HospitalApp.Infrastructure.Persistence;
using HospitalApp.Infrastructure.Persistence.Seeds;
using HospitalApp.WebAPI.BackgroundJobs;
using HospitalApp.WebAPI.Extensions;
using HospitalApp.WebAPI.GraphQL.Queries;
using HospitalApp.WebAPI.Hubs;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/lovasalud-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, services, config) => config
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/lovasalud-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddServices(builder.Configuration);

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")!)));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<ClinicBackgroundJobs>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<DashboardQuery>()
    .AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    ctx.Response.ContentType = "application/json";
    if (ex is HospitalApp.Core.Application.Common.Exceptions.ValidationException ve)
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsJsonAsync(new { errors = ve.Errors });
    }
    else
    {
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsJsonAsync(new { error = "Internal server error." });
    }
}));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-Frame-Options", "DENY");
    ctx.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    ctx.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    ctx.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; connect-src 'self' wss:; font-src 'self'; frame-ancestors 'none'");
    await next();
});

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DatabaseSeeder.SeedRolesAsync(roleManager, logger);
    await DatabaseSeeder.SeedAdminUserAsync(userManager, app.Configuration, logger);
}

app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
var hangfireUser = app.Configuration["HangfireDashboard:User"] ?? "admin";
var hangfirePass = app.Configuration["HangfireDashboard:Password"] ?? "changeme";
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireBasicAuthFilter(hangfireUser, hangfirePass)],
    DashboardTitle = "Lova Salud — Jobs",
});
app.MapControllers();
app.MapGraphQL("/graphql");
app.MapHub<DashboardHub>("/hubs/dashboard");

RecurringJob.AddOrUpdate<ClinicBackgroundJobs>(
    "mark-noshows", j => j.MarkNoShowAppointmentsAsync(), "*/30 * * * *");
RecurringJob.AddOrUpdate<ClinicBackgroundJobs>(
    "check-low-stock", j => j.CheckLowStockAsync(), Cron.Daily(7));
RecurringJob.AddOrUpdate<ClinicBackgroundJobs>(
    "void-stale-invoices", j => j.VoidStaleInvoicesAsync(), Cron.Daily(2));
RecurringJob.AddOrUpdate<ClinicBackgroundJobs>(
    "purge-audit-logs", j => j.PurgeOldAuditLogsAsync(), Cron.Weekly(DayOfWeek.Sunday, 3));
RecurringJob.AddOrUpdate<ClinicBackgroundJobs>(
    "send-scheduled-reports", j => j.SendScheduledReportsAsync(), "30 7 * * *");

app.Run();
