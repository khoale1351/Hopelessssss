using Microsoft.AspNetCore.Identity;

namespace Travel.Data
{
    public static class RoleInit
    {
        private static readonly string[] Roles = new[] { "Admin", "Customer", "Agent", "Guide", "Employeee", "Vendor" };

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
