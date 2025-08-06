using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.SeedData.SeedIssues.Data
{
    public class GapsInTransitionalServicesAfterFosterCarePrisonOrMilitaryService : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Gaps in Transitional Services After Foster Care, Prison, or Military Service",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 8, 17),
                    AuthorID = SeedUserTwelve.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = SystemicFailuresAndSafetyNets.ContentId // Making this a sub-issue of SystemicFailuresAndSafetyNets
                };
            }
        }

        public static Guid ContentId = new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d");
        public string content =
            "Many individuals leaving foster care, prison, or military service face significant challenges in transitioning " +
            "to stable, independent living. Gaps in transitional services often leave these populations without the support " +
            "needed to secure housing, employment, healthcare, and social connections, increasing their risk of homelessness " +
            "and long-term instability.\n\n" +

            "For youth aging out of foster care, the abrupt end of support can mean navigating adulthood without family, " +
            "financial resources, or guidance. Formerly incarcerated individuals encounter barriers to employment, housing, " +
            "and social reintegration, often compounded by stigma and legal restrictions. Veterans and those leaving military " +
            "service may struggle with mental health issues, physical injuries, and the challenge of adapting to civilian life.\n\n" +

            "Transitional programs are frequently underfunded, fragmented, or difficult to access. Eligibility requirements, " +
            "waitlists, and bureaucratic hurdles can prevent those most in need from receiving timely help. Coordination " +
            "between agencies is often lacking, resulting in missed opportunities for early intervention and support.\n\n" +

            "Addressing these gaps requires comprehensive, well-resourced transitional services that prioritize prevention, " +
            "empowerment, and long-term stability. Solutions include expanding case management, peer support, housing assistance, " +
            "job training, and mental health care tailored to the unique needs of each population. By strengthening transitional " +
            "services, we can reduce the risk of homelessness and promote successful reintegration into society.";

        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("71d8e054-9c3b-48a2-bf67-30e195d84a2c"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 18),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("fb249a5e-d07c-43e1-b8a5-36f1c094d782"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 19),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("42c83a9d-f150-4e8b-a7d2-9ef683b5c4f1"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 8, 20),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e5d09b37-c28f-46a4-95f0-1d7ce8a2b950"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3f2a96e0-d5c1-47b8-8ef3-4b7d98ca61a5"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("87ef24d1-9c56-48a7-b3f0-5e72d18c94ba"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2d639f18-75ab-4e20-9c84-f06d3b1c7a5e"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 8, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f5a37e84-20d9-48b1-936c-7a0be5d12c48"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 25),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("19c84b3e-6f5a-47d0-a2c1-9e87f0d3b542"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 26),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a63d9f28-7b4e-45c1-8f0a-e2d6b957c31f"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 27),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5d21e7f9-80b3-46a2-9c4d-1f8e0a7b6954"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 8, 28),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c8f49e27-35a1-4d60-b8e7-2f17d0a59c36"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 29),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("76b91d2a-4e5f-48c0-97d3-a8b6c5f2e70d"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 30),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3e9f7d12-0a5b-46c8-9d3e-f2a1b8c5d674"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 31),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("91c4e7d2-5a8f-49b3-0e6d-7f8a2b1c9e05"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 1),
               },
        };
    }
}
