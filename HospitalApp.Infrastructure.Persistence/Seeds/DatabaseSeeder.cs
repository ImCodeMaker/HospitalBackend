using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Infrastructure.Identity.Entities;
using HospitalApp.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HospitalApp.Infrastructure.Persistence.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedNcfRangesAsync(ApplicationDbContext db, ILogger logger)
    {
        if (await db.NcfSequences.AnyAsync()) return;

        var expiration = DateTime.UtcNow.AddYears(1);
        var seeds = new[]
        {
            new NcfSequence { Type = NcfTypeEnum.Consumo, CurrentSequence = 1, MaxSequence = 10_000, ExpirationDate = expiration },
            new NcfSequence { Type = NcfTypeEnum.CreditoFiscal, CurrentSequence = 1, MaxSequence = 2_000, ExpirationDate = expiration },
        };

        await db.NcfSequences.AddRangeAsync(seeds);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} NCF ranges (expires {Date:yyyy-MM-dd}).", seeds.Length, expiration);
    }

    public static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        var roles = new[]
        {
            ApplicationRole.Roles.Admin,
            ApplicationRole.Roles.Doctor,
            ApplicationRole.Roles.Receptionist,
            ApplicationRole.Roles.Cashier,
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
        var enabled = configuration.GetValue<bool>("SeedAdmin:Enabled");
        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];

        if (!enabled)
        {
            logger.LogInformation("Seed admin user skipped because SeedAdmin:Enabled is false.");
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Seed admin user skipped because SeedAdmin:Email or SeedAdmin:Password is missing.");
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, ApplicationRole.Roles.Admin))
            {
                var roleResult = await userManager.AddToRoleAsync(existingUser, ApplicationRole.Roles.Admin);
                if (roleResult.Succeeded)
                    logger.LogInformation("Ensured seed admin user has Admin role: {Email}", email);
                else
                    logger.LogWarning("Failed to add Admin role to seed user {Email}: {Errors}", email,
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
            return;
        }

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
