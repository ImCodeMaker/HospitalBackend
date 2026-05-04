using HospitalApp.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HospitalApp.Infrastructure.Persistence.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        var roles = new[]
        {
            ApplicationRole.Roles.Admin,
            ApplicationRole.Roles.Doctor,
            ApplicationRole.Roles.Receptionist,
            ApplicationRole.Roles.LabTechnician,
            ApplicationRole.Roles.Nurse,
            ApplicationRole.Roles.PatientPortal,
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new ApplicationRole { Name = role });
                if (result.Succeeded)
                    logger.LogInformation("Created role: {Role}", role);
                else
                    logger.LogWarning("Failed to create role {Role}: {Errors}", role,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    public static async Task SeedAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger logger)
    {
        var email = configuration["SeedAdmin:Email"] ?? "admin@lovasalud.com";
        var password = configuration["SeedAdmin:Password"] ?? "Admin@LovaSalud2026!";

        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Admin",
            LastName = "System",
            IsActive = true,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to seed admin user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, ApplicationRole.Roles.Admin);
        logger.LogInformation("Seeded admin user: {Email}", email);
    }
}
