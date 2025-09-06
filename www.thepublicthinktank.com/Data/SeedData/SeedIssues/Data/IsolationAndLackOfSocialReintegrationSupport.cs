using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class IsolationAndLackOfSocialReintegrationSupport : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Isolation and Lack of Social Reintegration Support",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 8, 20),
                    AuthorID = SeedUserEleven.user.Id, // Using centralized user ID
                    ScopeID = scope.ScopeID,
                    ParentIssueID = SystemicFailuresAndSafetyNets.ContentId // Making this a sub-issue of SystemicFailuresAndSafetyNets
                };
            }
        }

        public static Guid ContentId = new Guid("4b92c8d7-5e30-49a1-bf65-027d9c83e41a");
        public string content =
            "Social isolation and inadequate reintegration support represent significant challenges for individuals experiencing " +
            "or at risk of homelessness. Disconnection from supportive relationships, community networks, and social institutions " +
            "often exacerbates vulnerability, impedes recovery, and perpetuates cycles of marginalization and housing instability.\n\n" +

            "Many individuals facing homelessness have experienced the erosion of social ties due to various factors: family " +
            "conflict or breakdown, geographic displacement, stigma and discrimination, institutional transitions (such as leaving " +
            "foster care, prison, or hospitals), or the isolating effects of mental health conditions, substance use disorders, " +
            "or trauma. Without meaningful social connections and community integration, people struggle to access informal " +
            "support systems that might otherwise provide emotional sustenance, practical assistance, and pathways to housing " +
            "and employment opportunities.\n\n" +

            "Current service models often prioritize immediate material needs over long-term social integration, focusing on " +
            "crisis intervention rather than community building. Programs may inadvertently segregate vulnerable populations, " +
            "creating parallel systems that further disconnect individuals from mainstream social institutions. Meanwhile, " +
            "public spaces and community amenities increasingly employ hostile architecture and exclusionary practices that " +
            "physically reinforce social marginalization.\n\n" +

            "Addressing isolation requires comprehensive approaches that foster belonging, mutual support, and meaningful " +
            "participation in community life. Effective strategies include peer support programs, community integration specialists, " +
            "inclusive recreational and cultural activities, targeted outreach to reconnect people with estranged family members " +
            "when appropriate, and community education to reduce stigma. By strengthening social networks and promoting genuine " +
            "inclusion, we can help vulnerable individuals build the relationships and social capital essential for stable housing " +
            "and wellbeing.";

        public Scope scope
        {
            get
            {
                return new Scope()
                {
                    ScopeID = new Guid("38283341-1f14-4b8d-ab4c-f960037ea327"),
                    Scales = { Scale.Community, Scale.National },
                    Domains = { Domain.Health, Domain.Social },
                    EntityTypes = { EntityType.Organization, EntityType.Person, EntityType.Government },
                    Timeframes = { Timeframe.LongTerm, Timeframe.ShortTerm },
                    Boundaries = { },
                };
            }
        }
        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3a7c9d48-5e1b-46f2-a903-c82d57b14e90"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b5d8e7c6-94a3-47f1-8b20-d59e63c2a4f1"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 8, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("6f4d2a1c-8b97-45e3-a068-f2c53d7e90b8"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d17c9b8a-36e5-48f2-907d-4c3e5f2a1b90"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("497e5d3c-81a2-45f9-b6d0-3c7f8e95a21d"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 8, 25),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8c1b4a79-d6e5-47f3-9082-a5c73d2f9e10"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 26),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e2d1c9b8-7a53-46f4-80d9-5e3c7f2a1b90"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 27),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("59c8b7a6-4d3e-45f2-9081-a7c6d3f2e91b"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 28),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c1d9b8a7-5e4f-46d3-8072-9c5a3f2e1d90"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 8, 29),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("9e7d6c5b-4a3f-48d2-90c1-b7a6f5d4e3c2"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 8, 30),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2f1e9d8c-7b6a-45f5-80d4-3c2b1a9e7d5f"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 8, 31),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7a9c8b7d-6e5f-44d3-902c-1a8f7e6d5c4b"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 1),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d3c2b1a0-9e8d-47f6-80b5-7c6a5f4e3d2c"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 2),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8e7d6c5b-4a3f-42d1-90b8-7c6a5f4e3d2c"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 3),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("1f2e3d4c-5b6a-47f9-80d7-9c8b7a6f5e4d"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 9, 4),
               },
        };
    }
}