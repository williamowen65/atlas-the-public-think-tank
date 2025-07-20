using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class SecondChanceHiringIncentivesForEmployers : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = Homelessness.ContentId,
                    Title = "Second Chance Hiring Incentives for Employers",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 9, 10),
                    AuthorID = SeedUserEight.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a");
        public string content =
            "Second Chance Hiring Incentives represent a comprehensive approach to addressing employment barriers for individuals " +
            "transitioning from homelessness, incarceration, or extended unemployment. These programs create pathways to stable " +
            "employment—a critical factor in securing and maintaining housing.\n\n" +

            "The solution involves multi-faceted incentives for employers who hire qualified candidates with barriers to employment. " +
            "Tax credits form the foundation, offering businesses direct financial benefits for each eligible employee hired and " +
            "retained. Wage subsidies complement tax incentives by offsetting initial training costs during the critical onboarding " +
            "period when productivity may be developing. Bonding programs provide insurance protection against potential employee " +
            "dishonesty, removing a significant concern for employers considering candidates with criminal histories.\n\n" +

            "Beyond financial incentives, this approach includes support services that benefit both employers and employees: " +
            "specialized job coaches who provide ongoing mentorship; liaison services that help navigate workplace challenges; " +
            "and training grants that fund skill development tailored to specific industry needs. Recognition programs highlight " +
            "businesses demonstrating inclusive hiring practices, creating positive public relations opportunities.\n\n" +

            "Implementation requires collaboration between government agencies, community organizations, and the business " +
            "community. Streamlined application processes and clear eligibility guidelines are essential to encourage employer " +
            "participation. Success metrics should track not only initial placements but long-term retention and career advancement.\n\n" +

            "When properly structured, Second Chance Hiring Incentives create mutual benefits: employers gain motivated, loyal " +
            "employees and financial advantages, while vulnerable individuals secure economic self-sufficiency and stable housing. " +
            "Communities benefit from reduced homelessness, decreased recidivism, expanded tax bases, and the economic multiplier " +
            "effects of increased employment.";

        public SolutionVote[] solutionVotes { get; } = {
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("5a4b3c2d-1e0f-4a8b-9c7d-6e5f4d3c2b1a"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 11),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("6b5c4d3e-2f1a-4b9c-8d7e-5f4e3d2c1b0a"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 12),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("7c6d5e4f-3a2b-4c0d-9e8f-6a5f4e3d2c1b"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 13),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("8d7e6f5a-4b3c-4d1e-0f9a-7b6a5f4e3d2c"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 9, 14),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("9e8f7a6b-5c4d-4e2f-1a0b-8c7b6a5f4e3d"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 15),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("0f9a8b7c-6d5e-4f3a-2b1c-9d8c7b6a5f4e"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 16),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("1a0b9c8d-7e6f-4a4b-3c2d-0e9d8c7b6a5f"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 17),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("2b1c0d9e-8f7a-4b5c-4d3e-1f0e9d8c7b6a"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 18),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("3c2d1e0f-9a8b-4c6d-5e4f-2a1f0e9d8c7b"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 9, 19),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("4d3e2f1a-0b9c-4d7e-6f5a-3b2a1f0e9d8c"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 20),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("5e4f3a2b-1c0d-4e8f-7a6b-4c3b2a1f0e9d"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 21),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("6f5a4b3c-2d1e-4f9a-8b7c-5d4c3b2a1f0e"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 22),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("7a6b5c4d-3e2f-4a0b-9c8d-6e5d4c3b2a1f"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 9, 23),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("8b7c6d5e-4f3a-4b1c-0d9e-7f6e5d4c3b2a"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 24),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("9c8d7e6f-5a4b-4c2d-1e0f-8a7f6e5d4c3b"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 25),
               },
        };
    }
}