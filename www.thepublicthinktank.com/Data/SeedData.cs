using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Identity;

namespace atlas_the_public_think_tank.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context,
    UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            Console.WriteLine("TODO: Seed data");

            await SeedDefaultRoles(roleManager);
         
            //// Seed default users
            await SeedDefaultUsers(userManager);

            await SeedCategories(context);
        }

        public static async Task SeedCategories(ApplicationDbContext context)
        {
            if (context.Categories.Any())
            {
                return; // DB has already been seeded with categories
            }

            var categories = new List<Category>
            {
            new Category { CategoryName = "Global Cooperation" },
            new Category { CategoryName = "Sustainable Development" },
            new Category { CategoryName = "Equitable Access" },
            new Category { CategoryName = "Innovation and Technology" },
            new Category { CategoryName = "Effective Governance" },
            new Category { CategoryName = "Education and Awareness" },
            new Category { CategoryName = "Cultural Understanding" },
            new Category { CategoryName = "Resilience and Adaptability" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }

        public static async Task SeedScopes(ApplicationDbContext context)
        {
            if (context.Scopes.Any())
            {
                return; // DB has already been seeded with scopes
            }

            var scopes = new List<Scope>
        {
            new Scope { ScopeName = "Global" },
            new Scope { ScopeName = "National" },
            new Scope { ScopeName = "Local" },
            new Scope { ScopeName = "Individual" }
        };

            context.Scopes.AddRange(scopes);
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
