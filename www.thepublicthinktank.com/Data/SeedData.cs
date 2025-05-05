using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Identity;

namespace atlas_the_public_think_tank.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context,
    UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            await SeedDefaultRoles(roleManager);
         
            // Seed default users
            await SeedDefaultUsers(userManager);

            await SeedCategories(context);
        }

        public static async Task SeedCategories(ApplicationDbContext context)
        {
            // Check if categories already exist
            if (context.Categories.Any())
            {
                return;   // DB has already been seeded with categories
            }

            var categories = new List<Category>
            {
                new Category { 
                    Name = "Global Cooperation" ,
                    Description = "",
                     CreatedAt = DateTime.UtcNow
                },
                new Category { 
                    Name = "Sustainable Development" ,
                    Description = "",
                     CreatedAt = DateTime.UtcNow
                },
                new Category { 
                    Name = "Equitable Access" ,
                    Description = "",
                     CreatedAt = DateTime.UtcNow
                },
                new Category { 
                    Name = "Innovation and Technology" ,
                    Description = "",
                     CreatedAt = DateTime.UtcNow
                },
                new Category { 
                    Name = "Effective Governance" ,
                    Description = "",
                     CreatedAt = DateTime.UtcNow
                },
                new Category { 
                    Name = "Education and Awareness" ,
                    Description = "",
                     CreatedAt = DateTime.UtcNow
                },
                new Category { 
                    Name = "Cultural Understanding" ,
                    Description = "",
                     CreatedAt = DateTime.UtcNow
                },
                new Category { 
                    Name = "Resilience and Adaptability",
                    Description = "",
                     CreatedAt = DateTime.UtcNow
                 }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }   

        public static async Task SeedDefaultRoles(RoleManager<IdentityRole> roleManager)
        {
            // Create roles if they don't exist
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
        }

        public static async Task SeedDefaultUsers(UserManager<AppUser> userManager)
        {
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
