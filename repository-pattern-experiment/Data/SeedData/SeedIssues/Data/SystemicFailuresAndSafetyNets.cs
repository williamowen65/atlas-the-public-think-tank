using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.SeedData.SeedIssues.Data
{
    public class SystemicFailuresAndSafetyNets : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Systemic Failures and Safety Nets",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 8, 1),
                    AuthorID = SeedUserTen.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = Homelessness.ContentId // Making this a sub-issue of Homelessness
                };
            }
        }

        public static Guid ContentId = new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39");
        public string content =
            "Homelessness and housing instability are not just the result of individual circumstances, but often reflect deeper " +
            "systemic failures and gaps in the social safety net. When institutions designed to protect vulnerable populations " +
            "break down, individuals and families can quickly fall through the cracks, facing cycles of poverty, instability, " +
            "and exclusion.\n\n" +

            "Key failures include insufficient access to mental health care, addiction treatment, and preventive health services; " +
            "inadequate unemployment insurance and income support; lack of affordable childcare; and fragmented or underfunded " +
            "transitional services for those leaving foster care, prison, or military service.\n\n" +

            "Bureaucratic barriers, eligibility restrictions, and complex application processes often prevent those most in need " +
            "from accessing help. Many safety net programs are reactive rather than proactive, intervening only after crises " +
            "have escalated. Coordination between agencies is frequently poor, resulting in duplicated efforts, missed opportunities, " +
            "and gaps in care.\n\n" +

            "Addressing these systemic failures requires a holistic approach: investing in robust, accessible safety nets; " +
            "streamlining service delivery; prioritizing prevention and early intervention; and ensuring that support systems " +
            "are trauma-informed, culturally competent, and responsive to the needs of diverse populations. By strengthening " +
            "the social safety net, we can reduce the risk of homelessness and promote greater stability and opportunity for all.";

        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("9c5d8e63-7f21-48b4-a1e9-f30d762b85c1"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 2),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("87a1bf23-c64d-40e5-b9a7-15f2d8c09e4a"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 8, 3),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3e5f7c82-1abd-42f9-8e6b-0d94c3a58f27"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 4),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d2a6b9f1-4c53-47e0-9387-61a50fc8d249"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 5),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f18d0c37-6a95-48be-921f-5e4a7b9d0c38"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 8, 6),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("4e9db5a7-08c1-49f2-b3e6-7d82510f6ca9"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 7),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2b7c1e49-5ad3-4f06-98e2-0c31fb57a8d4"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 8),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b5c9a2e7-3d14-46f8-95a0-7183df462c01"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 9),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("6d9e0f34-82a5-4b1c-b7d8-5e30c9f16a72"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 8, 10),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("1f8a3c54-09be-47d6-a2c7-835fb940d6e9"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 11),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c0e7d59a-42f1-48b3-9165-7a84f3d20b8c"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 12),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7db92e45-3af6-4c18-b507-29d1e6085fca"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 13),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e1f23c7a-9b56-48d0-a4e7-0c3d9f82b615"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 8, 14),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("93a5b4c0-1d7e-46f8-b29a-5c06e874d3f2"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 15),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5d2e8b47-a0f3-491c-b6d5-e7940c38af26"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 16),
               },
        };
    }
}