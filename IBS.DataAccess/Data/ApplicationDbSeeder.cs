using IBS.Models;
using IBS.Models.MasterFile;
using IBS.Utility.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IBS.DataAccess.Data
{
    public static class ApplicationDbSeeder
    {
        private static readonly string[] _roles = ["Admin", "User"];

        private const string _adminUserName = "azh";
        private const string _adminPassword = "Testing.1234";

        public static async Task SeedAsync(IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await dbContext.Database.MigrateAsync(cancellationToken);

            foreach (var role in _roles)
            {
                if (await roleManager.RoleExistsAsync(role))
                {
                    continue;
                }

                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Unable to seed role '{role}': {errors}");
                }
            }

            var adminUser = await userManager.FindByNameAsync(_adminUserName);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = _adminUserName,
                    Email = "admin@ibs.local",
                    EmailConfirmed = true,
                    Name = "AZH ADOLFO",
                    Department = SD.Department_MIS,
                    IsActive = true
                };

                var createUserResult = await userManager.CreateAsync(adminUser, _adminPassword);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Unable to seed admin user '{_adminUserName}': {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addToRoleResult.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Unable to assign Admin role to '{_adminUserName}': {errors}");
                }
            }
        }
    }
}
