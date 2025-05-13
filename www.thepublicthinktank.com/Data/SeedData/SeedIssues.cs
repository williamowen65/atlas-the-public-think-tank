using atlas_the_public_think_tank.Models;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedIssues
    {

        public SeedIssues(ModelBuilder modelBuilder)
        {

            SeedUsers.UserIds.TryGetValue("user1", out string userId1);
            SeedUsers.UserIds.TryGetValue("user2", out string userId2);

            modelBuilder.Entity<Issue>().HasData(
                new Issue {
                    IssueID = 1,
                    Title = "Climate Change Solutions",
                    Content = "A issue to discuss practical solutions to climate change at individual and policy levels.",
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 15),
                    AuthorID = userId1, // Match an existing user ID
                    ScopeID = 1 // Match an existing scope ID
                },
                new Issue {
                    IssueID = 2,
                    Title = "Urban Planning Innovations",
                    Content = "Discussion on modern urban planning approaches for sustainable and livable cities.",
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 2, 10),
                    AuthorID = userId1, // Match an existing user ID
                    ScopeID = 2 // Match an existing scope ID
                },
                new Issue {
                    IssueID = 3,
                    Title = "Renewable Energy Transition",
                    Content = "Strategies for transitioning to renewable energy sources at community and national levels.",
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 3, 5),
                    ModifiedAt = new DateTime(2024, 3, 10),
                    AuthorID = userId2, // Match an existing user ID
                    ScopeID = 1, // Match an existing scope ID
                    ParentIssueID = 1 // This is a sub-issue of issue #1
                }  
            );
        }
    }
}
