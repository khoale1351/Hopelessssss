using Microsoft.AspNetCore.Identity;
using Travel.Models;

namespace Travel.Data
{
    public class DataSeeder
    {
        public static async Task SeedAdmin(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@admin.com";
            string adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "@admin_qtv@";

            // Tạo role Admin nếu chưa có
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                    Console.WriteLine("Admin account created successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to create admin account:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(error.Description);
                    }
                }
            }
        }
    }
}
