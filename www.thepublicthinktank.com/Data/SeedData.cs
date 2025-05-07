using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context,
    UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            Console.WriteLine("Seeding data...");

            await SeedDefaultRoles(roleManager);
         
            // Seed default users
            await SeedDefaultUsers(userManager);

            await SeedCategories(context);
            
            await SeedScopes(context);
            
            // Add seed data using stored procedures
            await SeedDataWithStoredProcedures(context, userManager);

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

        public static async Task SeedDataWithStoredProcedures(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            // Skip if forums or solutions already exist 
            if (context.Forums.Any() || context.Solutions.Any())
            {
                Console.WriteLine("Forums or solutions already exist, skipping stored procedure seeding");
                return;
            }

            // Get user IDs for the stored procedure calls
            var user1 = await userManager.FindByEmailAsync("admin@example.com");
            var user2 = await userManager.FindByEmailAsync("user@example.com");
            var user3 = await userManager.FindByEmailAsync("user3@example.com");
            var user4 = await userManager.FindByEmailAsync("user4@example.com");

            if (user1 == null || user2 == null || user3 == null || user4 == null)
            {
                throw new Exception("Some users don't exist. Make sure to seed users first.");
            }

            // Connection string from the context
            var connectionString = context.Database.GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Unable to get database connection string");
            }

            // Execute stored procedure calls
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Declare variable for tracking inserted ForumID
                using (var command = new SqlCommand("DECLARE @forumID_INSERT int;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                // Forum 2 - The Public Think Tank
                using (var command = new SqlCommand(
                    @"EXEC @forumID_INSERT = spForums_INSERT
                        @ForumID = 2,
                        @Title = 'The Public Think Tank: What features would improve it?',
                        @Description = 'Social media can play a significant role in helping \
humanity reach goals for a cohesive and thriving global society, provided it \
is used effectively and responsibly. \
Here's how social media can contribute:\
\
1. **Raising Awareness:**  \
   Social media platforms can spread awareness about important \
   issues such as climate change, inequality, and health \
   crises. Campaigns and informational content can reach large \
   audiences quickly and mobilize action.\
\
2. **Facilitating Communication:**  \
   Social media enables global communication and collaboration, \
   allowing people from different regions and backgrounds to \
   connect, share ideas, and work together on common goals.\
\
3. **Supporting Activism:**  \
   Social media has been a powerful tool for grassroots \
   movements and activism. It can help organize events, rally \
   support, and drive social and political change.\
\
4. **Crowdsourcing Solutions:**  \
   Platforms can be used to gather input and ideas from diverse \
   populations, leveraging collective intelligence to solve \
   problems and innovate.\
\
5. **Educational Outreach:**  \
   Social media can provide access to educational resources, \
   expert knowledge, and learning opportunities. It can also \
   support digital literacy and critical thinking.\
\
6. **Community Building:**  \
   Social media helps build and strengthen communities around \
   shared interests and causes. Online communities can offer \
   support, share resources, and amplify voices.\
\
7. **Promoting Transparency and Accountability:**  \
   Social media can be used to monitor and report on issues \
   such as corruption, environmental damage, and human rights \
   abuses. It provides a platform for holding individuals and \
   institutions accountable.\
\
8. **Disseminating Critical Information:**  \
   In times of crisis, social media can quickly disseminate \
   important information, provide updates, and coordinate \
   responses.\
\
However, there are challenges and risks associated with social \
media use:\
\
- **Misinformation and Fake News:**  \
  The spread of misinformation can undermine efforts and create \
  confusion. It's crucial to verify information and promote \
  media literacy.\
\
- **Privacy and Security:**  \
  Ensuring user privacy and data security is essential to \
  maintaining trust and protecting individuals from harm.\
  \
- **Echo Chambers:**  \
  Social media can create echo chambers where users are exposed \
  only to information that reinforces their existing beliefs. \
  This can hinder constructive dialogue and understanding.\
\
- **Digital Divide:**  \
  Access to social media and digital technologies is uneven \
  across the globe, which can exacerbate inequalities.\
\
By addressing these challenges and using social media \
strategically, it can be a valuable tool in promoting positive \
change and helping humanity achieve its goals.',
                        @AuthorID = @user3Id,
                        @ScopeID = 2,
                        @ParentForumID = NULL,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user3Id", user3.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Add categories to Forum 2
                using (var command = new SqlCommand(
                    "EXEC spForumsCategories_INSERT @CategoryName = 'Global Cooperation', @ForumID = 2;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new SqlCommand(
                    "EXEC spForumsCategories_INSERT @CategoryName = 'Innovation and Technology', @ForumID = 2;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                // Add solutions to Forum 2
                using (var command = new SqlCommand(
                    @"EXEC spSolutions_INSERT
                        @SolutionID = 3,
                        @ForumID = 2,
                        @Title = 'Make sure to handle multiple languages',
                        @Description = 'If the site is meant to be a global public think tank, then it should handle other language characters and be able to translate languages for different clients. (use nvarchar)',
                        @AuthorID = @user1Id,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user1Id", user1.Id);
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new SqlCommand(
                    @"EXEC spSolutions_INSERT
                        @SolutionID = 2,
                        @ForumID = 2,
                        @Title = 'Add a more expressive way to explain problems and solutions',
                        @Description = 'I''m, specifically thinking Google Docs platform would be grate to work with. Maybe there are ways to integrate them directly into HTML. \
        The only problems I can forsee are that the content of them might not be searchable from the Public Think Tank.',
                        @AuthorID = @user1Id,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user1Id", user1.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Add votes to Forum 2's solution
                using (var command = new SqlCommand(
                    @"EXEC spUserVotes_INSERT
                        @ForumID = 2,
                        @ForumSolutionID = 2,
                        @CommentID = 0,
                        @UserID = @user3Id,
                        @Vote = 10", connection))
                {
                    command.Parameters.AddWithValue("@user3Id", user3.Id);
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new SqlCommand(
                    @"EXEC spUserVotes_INSERT
                        @ForumID = 2,
                        @ForumSolutionID = 2,
                        @CommentID = 0,
                        @UserID = @user4Id,
                        @Vote = 2", connection))
                {
                    command.Parameters.AddWithValue("@user4Id", user4.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Forum 3 - Endangered Species
                using (var command = new SqlCommand(
                    @"EXEC @forumID_INSERT = spForums_INSERT
                        @ForumID = 3,
                        @Title = 'How can we restore populations of endangered species?',
                        @Description = 'For example, the Southern Resident Orcas of the San Juan Islands',
                        @AuthorID = @user3Id,
                        @ScopeID = 3,
                        @ParentForumID = NULL,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user3Id", user3.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Add category to Forum 3
                using (var command = new SqlCommand(
                    "EXEC spForumsCategories_INSERT @CategoryName = 'Biodiversity and Wildlife', @ForumID = 3;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                // Forum 4 - Sub-problem of Forum 3
                using (var command = new SqlCommand(
                    @"EXEC @forumID_INSERT = spForums_INSERT
                        @ForumID = 4,
                        @Title = 'How can we improve public education around endangered species?',
                        @Description = 'This seems like the way to generate public investment in biodiversity and wildlife.',
                        @AuthorID = @user4Id,
                        @ScopeID = 2,
                        @ParentForumID = 3,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user4Id", user4.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Add category to Forum 4
                using (var command = new SqlCommand(
                    "EXEC spForumsCategories_INSERT @CategoryName = 'Biodiversity and Wildlife', @ForumID = 4;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                // Forum 1 - Driving safety
                using (var command = new SqlCommand(
                    @"EXEC @forumID_INSERT = spForums_INSERT
                        @ForumID = 1,
                        @Title = 'How can we reduce the risk and danger associated with driving on the road?',
                        @Description = 'Every time I am on the road I see people driving with out regard to safety. I know there are stats on driving accidents and that they can be reduced. What are some ways we can make that happpen?',
                        @AuthorID = @user3Id,
                        @ScopeID = 2,
                        @ParentForumID = NULL,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user3Id", user3.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Add category to Forum 1
                using (var command = new SqlCommand(
                    "EXEC spForumsCategories_INSERT @CategoryName = 'Resilience and Adaptability', @ForumID = 1;", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                // Update Forum 1 title
                using (var command = new SqlCommand(
                    @"EXEC spForums_UPDATE 
                        @ForumID = 1,
                        @Title = 'How can we reduce the risk and danger associated with driving on the freeway?',
                        @Description = 'Every time I am on the road I see people driving with out regard to safety. I know there are stats on driving accidents and that they can be reduced. What are some ways we can make that happpen?',
                        @AuthorID = @user3Id,
                        @ScopeID = 2,
                        @ParentForumID = NULL,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user3Id", user3.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Add comment to Forum 1
                using (var command = new SqlCommand(
                    @"EXEC spComments_INSERT
                        @CommentID = 1,
                        @Comment = 'Sounds like an idea the Mechatronics people could help build ',
                        @ForumID = 1,
                        @SolutionID = NULL,
                        @AuthorID = @user2Id,
                        @ParentCommentID = NULL,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user2Id", user2.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Update comment
                using (var command = new SqlCommand(
                    @"EXEC spComments_UPDATE
                        @CommentID = 1,
                        @Comment = 'Sounds like an idea the Mechatronics people could help build. Maybe we should inquire about that.',
                        @ForumID = 1,
                        @SolutionID = NULL,
                        @AuthorID = @user2Id,
                        @ParentCommentID = NULL,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user2Id", user2.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Add vote to comment
                using (var command = new SqlCommand(
                    @"EXEC spUserVotes_INSERT
                        @ForumID = 1,
                        @ForumSolutionID = 0,
                        @CommentID = 1,
                        @UserID = @user3Id,
                        @Vote = 10", connection))
                {
                    command.Parameters.AddWithValue("@user3Id", user3.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Add solution to Forum 1
                using (var command = new SqlCommand(
                    @"EXEC spSolutions_INSERT
                        @SolutionID = 1,
                        @ForumID = 1,
                        @Title = 'Add a device to cars which reports tail-gaters',
                        @Description = 'The pieces of tech already exist, they just need to be assembled in the right way. We already have back up cams. Why can''t those also be used to take pictures of license plates tail gating you on the freeway, with automated reporting? If something like this was mainstream and people knew about it, there would be less tail gaters.',
                        @AuthorID = @user3Id,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user3Id", user3.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Update solution
                using (var command = new SqlCommand(
                    @"EXEC spSolutions_UPDATE
                        @SolutionID = 1,
                        @ForumID = 1,
                        @Title = 'Add a device to cars which automatically reports tail-gaters when on the freeway',
                        @Description = 'The pieces of tech already exist, they just need to be assembled in the right way. We already have back up cams. Why can''t those also be used to take pictures of license plates tail gating you on the freeway, with automated reporting? If something like this was mainstream and people knew about it, there would be less tail gaters.',
                        @AuthorID = @user3Id,
                        @BlockedContentID = NULL", connection))
                {
                    command.Parameters.AddWithValue("@user3Id", user3.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Add forum votes
                using (var command = new SqlCommand(
                    @"EXEC spUserVotes_INSERT
                        @ForumID = 1,
                        @ForumSolutionID = 0,
                        @CommentID = 0,
                        @UserID = @user2Id,
                        @Vote = 10", connection))
                {
                    command.Parameters.AddWithValue("@user2Id", user2.Id);
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new SqlCommand(
                    @"EXEC spUserVotes_INSERT
                        @ForumID = 1,
                        @ForumSolutionID = 0,
                        @CommentID = 0,
                        @UserID = @user1Id,
                        @Vote = 7", connection))
                {
                    command.Parameters.AddWithValue("@user1Id", user1.Id);
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new SqlCommand(
                    @"EXEC spUserVotes_INSERT
                        @ForumID = 1,
                        @ForumSolutionID = 0,
                        @CommentID = 0,
                        @UserID = @user4Id,
                        @Vote = 6", connection))
                {
                    command.Parameters.AddWithValue("@user4Id", user4.Id);
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new SqlCommand(
                    @"EXEC spUserVotes_INSERT
                        @ForumID = 2,
                        @ForumSolutionID = 0,
                        @CommentID = 0,
                        @UserID = @user4Id,
                        @Vote = 9", connection))
                {
                    command.Parameters.AddWithValue("@user4Id", user4.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // Add solution votes
                using (var command = new SqlCommand(
                    @"EXEC spUserVotes_INSERT
                        @ForumID = 1,
                        @ForumSolutionID = 1,
                        @CommentID = 0,
                        @UserID = @user2Id,
                        @Vote = 8", connection))
                {
                    command.Parameters.AddWithValue("@user2Id", user2.Id);
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new SqlCommand(
                    @"EXEC spUserVotes_INSERT
                        @ForumID = 1,
                        @ForumSolutionID = 1,
                        @CommentID = 0,
                        @UserID = @user1Id,
                        @Vote = 7", connection))
                {
                    command.Parameters.AddWithValue("@user1Id", user1.Id);
                    await command.ExecuteNonQueryAsync();
                }

                // This duplicate vote is expected to fail as per the comment in the SQL script
                try
                {
                    using (var command = new SqlCommand(
                        @"EXEC spUserVotes_INSERT
                            @ForumID = 1,
                            @ForumSolutionID = 1,
                            @CommentID = 0,
                            @UserID = @user1Id,
                            @Vote = 7", connection))
                    {
                        command.Parameters.AddWithValue("@user1Id", user1.Id);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Expected duplicate vote failure: {ex.Message}");
                }
            }

            Console.WriteLine("Stored procedure seed data added successfully");
        }
    }
}
