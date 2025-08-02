using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class WorkforceAutomationAndJobDisplacement : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Workforce Automation and Job Displacement",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 20),
                    AuthorID = SeedUserThirteen.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3");
        public string content =
            "Rapid advancements in artificial intelligence, robotics, and automation are fundamentally reshaping the global " +
            "workforce, creating both unprecedented opportunities and significant challenges. As machines increasingly " +
            "perform tasks traditionally done by humans, from manufacturing and customer service to data analysis and " +
            "medical diagnostics, millions of workers face potential displacement and uncertain futures.\n\n" +

            "This technological revolution disproportionately impacts certain sectors and demographics, particularly " +
            "routine-based occupations and workers without advanced education or specialized skills. While automation " +
            "creates new types of jobs, there's growing concern about whether enough new positions will emerge to " +
            "compensate for those eliminated, and whether displaced workers can successfully transition to these new roles.\n\n" +

            "The social and economic implications extend beyond individual livelihoods to affect entire communities, " +
            "potentially widening inequality and creating social instability. How can we harness the productivity and " +
            "innovation of automation while ensuring its benefits are broadly shared? What policies, educational reforms, " +
            "and social safety nets might help workers navigate this shifting landscape? And how do we balance technological " +
            "progress with human dignity and economic security?\n\n" +

            "These questions demand urgent attention from policymakers, business leaders, educators, and citizens as we " +
            "collectively shape how automation will transform not just our economy, but the very nature of work itself.";

        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e7f8d9c0-a1b2-4c3d-9e8f-7a6b5c4d3e2f"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 20),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f8e9d0c1-b2a3-4d5e-0f1a-2b3c4d5e6f7a"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 1, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 6,
                    CreatedAt = new DateTime(2024, 1, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 25),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e5f6a7b8-c9d0-1e2f-3a4b-5c6d7e8f9a0b"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 5,
                    CreatedAt = new DateTime(2024, 1, 26),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f6a7b8c9-d0e1-2f3a-4b5c-6d7e8f9a0b1c"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 27),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a7b8c9d0-e1f2-3a4b-5c6d-7e8f9a0b1c2d"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 1, 28),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b8c9d0e1-f2a3-4b5c-6d7e-8f9a0b1c2d3e"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 6,
                    CreatedAt = new DateTime(2024, 1, 29),
               },
        };
    }
}
