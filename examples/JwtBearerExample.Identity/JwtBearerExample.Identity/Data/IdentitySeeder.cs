using Microsoft.AspNetCore.Identity;

namespace JwtBearerExample.Identity.Data;

public static class IdentitySeeder
{
    private static readonly (string Email, string Password, string[] Roles)[] SeedUsers =
    {
        ("admin@local.dev", "Admin123!", ["Admin", "Viewer"]),
        ("support@local.dev", "Support123!", ["Support", "Viewer"]),
        ("viewer@local.dev", "Viewer123!", ["Viewer"]),
        ("ops@local.dev", "Ops12345!", ["Admin", "Support"])
    };

    private static readonly string[] SeedRoles = ["Admin", "Support", "Viewer"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var role in SeedRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (!createRoleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create role '{role}': {string.Join(", ", createRoleResult.Errors.Select(e => e.Description))}");
                }
            }
        }

        foreach (var seedUser in SeedUsers)
        {
            var user = await userManager.FindByEmailAsync(seedUser.Email);
            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = seedUser.Email,
                    Email = seedUser.Email,
                    EmailConfirmed = true
                };

                var createUserResult = await userManager.CreateAsync(user, seedUser.Password);
                if (!createUserResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create user '{seedUser.Email}': {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                }
            }

            var existingRoles = await userManager.GetRolesAsync(user);
            var missingRoles = seedUser.Roles.Except(existingRoles, StringComparer.OrdinalIgnoreCase).ToArray();
            if (missingRoles.Length > 0)
            {
                var addRolesResult = await userManager.AddToRolesAsync(user, missingRoles);
                if (!addRolesResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to assign roles to '{seedUser.Email}': {string.Join(", ", addRolesResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
