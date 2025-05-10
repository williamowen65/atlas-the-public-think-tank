using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace atlas_the_public_think_tank.Data
{
    public static partial class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context,
    UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            Console.WriteLine("Seeding data...");

            await SeedDefaultRoles(roleManager);
         
            // Seed default users
            await SeedDefaultUsers(userManager);

            await SeedCategories(context);
            
            await SeedScopes(context);
            
            // // Add seed data using stored procedures
            //  await SeedDataMiscData(context, userManager, config);

            // await SeedDefaultEntriesForVoteTable(context, userManager);

            Console.WriteLine("Seeding complete!");
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
            // Create users specified in SQL script and existing ones
            var users = new List<(string Email, string Password, bool IsAdmin)>
            {
                ("admin@example.com", "Admin123!", true),     // Original admin (ID 1 in SQL)
                ("user@example.com", "User123!", false),      // Original user (ID 2 in SQL)
                ("user3@example.com", "User123!", false),     // ID 3 in SQL
                ("user4@example.com", "User123!", false)      // ID 4 in SQL
            };

            foreach (var (email, password, isAdmin) in users)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new AppUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded && isAdmin)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }
            }
        }


        // There need to be an entry in related tables for a "system" entry.
        // This entry is needed because UserVotes has a composite primary key,
        // and primary keys cannot be null
        // So this method creates system enrty (ID = 0)
        // For:
        // Forums, Solutions, Comments,
        //
        // when votes are added to the table, 0 can be placed instead of null
        // when the vote applies to another type.
        public static async Task SeedDefaultEntriesForVoteTable(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            // Retrieve the admin user
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                throw new Exception("Admin user not found. Ensure the admin user is seeded before calling this method.");
            }
            var adminId = adminUser.Id;

            var scope = await context.Scopes.FirstOrDefaultAsync();


            // Check if the "system" entry already exists in Forums
            if (!context.Forums.Any(f => f.ForumID == 0))
            {
                context.Forums.Add(new Forum
                {
                    ForumID = 0,
                    Title = "System Entry",
                    Content = "This is a system entry for votes.",
                    CreatedAt = DateTime.UtcNow,
                    ContentStatus = ContentStatus.Archived,
                    AuthorID = adminId,
                    ScopeID = scope.ScopeID
                });
                // Save changes to the database
                await context.SaveChangesAsync();
            }

            //// Check if the "system" entry already exists in Solutions
            if (!context.Solutions.Any(s => s.SolutionID == 0))
            {
                context.Solutions.Add(new Solution
                {
                    SolutionID = 0,
                    Title = "System Entry",
                    Content = "This is a system entry for votes.",
                    CreatedAt = DateTime.UtcNow,
                    ContentStatus = ContentStatus.Archived,
                    AuthorID = adminId,
                    ForumID = 0
                });
                // Save changes to the database
                await context.SaveChangesAsync();
            }

            //// Check if the "system" entry already exists in Comments
            if (!context.Comments.Any(c => c.CommentID == 0))
            {
                context.Comments.Add(new UserComment
                {
                    CommentID = 0,
                    Comment = "This is a system entry for votes.",
                    CreatedAt = DateTime.UtcNow,
                    ContentStatus = ContentStatus.Archived,
                    AuthorID = adminId
                });
                // Save changes to the database
                await context.SaveChangesAsync();
            }

            if (!context.UserVotes.Any()) {
                context.UserVotes.Add(new UserVote
                {
                    CommentID = 0,
                    ForumID = 0,
                    UserID = adminId,
                    ForumSolutionID = 0
                });
                await context.SaveChangesAsync();
            }


        }



    }
}
