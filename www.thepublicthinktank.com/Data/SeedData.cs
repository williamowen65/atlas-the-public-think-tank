using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Identity;

namespace atlas_the_public_think_tank.Data
{
   public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context, 
        UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Create roles if they don't exist
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            
            // Create admin user if it doesn't exist
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var adminUser = new AppUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            
            // Create regular user if it doesn't exist
            if (await userManager.FindByEmailAsync("user@example.com") == null)
            {
                var regularUser = new AppUser
                {
                    UserName = "user@example.com",
                    Email = "user@example.com",
                    EmailConfirmed = true
                };
                
                await userManager.CreateAsync(regularUser, "User123!");
            }
        }
    }
}
