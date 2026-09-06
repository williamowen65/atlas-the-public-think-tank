using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class StigmaPreventingPeopleFromSeekingHelp : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Stigma Preventing People From Seeking Help",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 6, 10),
                    AuthorID = SeedUserEight.user.Id, // Using centralized user ID
                    ScopeID = scope.ScopeID, 
                    ParentIssueID = Homelessness.ContentId // Making this a sub-issue of Homelessness
                };
            }
        }

        public static Guid ContentId = new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b");
        public string content =
            "Social stigma remains one of the most persistent yet under-addressed barriers preventing individuals experiencing " +
            "homelessness from seeking and accessing available support services. This stigmatization manifests in various forms " +
            "that significantly impact both individual behavior and institutional responses.\n\n" +

            "At the individual level, stigma often leads to feelings of shame, embarrassment, and diminished self-worth among " +
            "those experiencing homelessness. These emotional burdens can cause people to avoid seeking services, hide their " +
            "housing status, or refuse to identify themselves as homeless—even when doing so would connect them with crucial " +
            "resources. Many report fears of being judged, discriminated against, or treated disrespectfully when interacting " +
            "with service providers, healthcare facilities, or government agencies.\n\n" +

            "Public misconceptions about homelessness frequently center on assumptions that it results primarily from personal " +
            "failings rather than systemic issues like housing unaffordability, poverty, inadequate mental health care, and " +
            "other structural factors. These misunderstandings further reinforce stigma and can lead to dehumanizing treatment " +
            "of homeless individuals in public spaces and service settings.\n\n" +

            "Institutional practices often inadvertently perpetuate stigma through bureaucratic procedures, intrusive questioning, " +
            "or service environments that lack dignity and privacy. Many homeless services operate from a deficit model that " +
            "emphasizes compliance rather than empowerment, further alienating potential clients.\n\n" +

            "Addressing stigma requires coordinated approaches, including public education campaigns, trauma-informed care " +
            "training for service providers, peer support models that employ formerly homeless individuals, and service " +
            "design that prioritizes dignity and self-determination. By tackling the invisible barrier of stigma, we can " +
            "significantly improve service utilization and effectiveness in addressing homelessness.";


        public Scope scope
        {
            get
            {
                return new Scope()
                {
                    ScopeID = new Guid("d3415df0-9ae6-450f-8665-eacd297d2ddc"),
                    Scales = { Scale.National, Scale.Community },
                    Domains = { Domain.Health, Domain.Social, Domain.Cultural },
                    EntityTypes = { EntityType.Organization, EntityType.Person, EntityType.Government },
                    Timeframes = { Timeframe.LongTerm, Timeframe.ShortTerm },
                    Boundaries = { },
                };
            }
        }
        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a1b2c3d4-e5f6-47a8-b9c0-d1e2f3a4b5c6"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 6, 11),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b2c3d4e5-f6a7-48b9-c0d1-e2f3a4b5c6d7"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 6, 12),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c3d4e5f6-a7b8-49c0-d1e2-f3a4b5c6d7e8"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 6, 13),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d4e5f6a7-b8c9-40d1-e2f3-a4b5c6d7e8f9"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 6, 14),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e5f6a7b8-c9d0-41e2-f3a4-b5c6d7e8f9a0"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 6, 15),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f6a7b8c9-d0e1-42f3-a4b5-c6d7e8f9a0b1"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 6, 16),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a7b8c9d0-e1f2-43a4-b5c6-d7e8f9a0b1c2"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 6, 17),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b8c9d0e1-f2a3-44b5-c6d7-e8f9a0b1c2d3"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 6, 18),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c9d0e1f2-a3b4-45c6-d7e8-f9a0b1c2d3e4"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 6, 19),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d0e1f2a3-b4c5-46d7-e8f9-a0b1c2d3e4f5"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 6, 20),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e1f2a3b4-c5d6-47e8-f9a0-b1c2d3e4f5a6"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 6, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f2a3b4c5-d6e7-48f9-a0b1-c2d3e4f5a6b7"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 6, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a3b4c5d6-e7f8-49a0-b1c2-d3e4f5a6b7c8"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 6, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b4c5d6e7-f8a9-40b1-c2d3-e4f5a6b7c8d9"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 6, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c5d6e7f8-a9b0-41c2-d3e4-f5a6b7c8d9e0"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 6, 25),
               },
        };
    }
}
