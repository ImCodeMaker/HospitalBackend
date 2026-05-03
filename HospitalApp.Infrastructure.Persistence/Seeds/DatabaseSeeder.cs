using HospitalApp.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
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
}
