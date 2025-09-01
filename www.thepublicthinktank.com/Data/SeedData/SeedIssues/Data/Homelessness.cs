using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class Homelessness : SeedIssueContainer
    {



        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Homelessness",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 1, 15),
                    AuthorID = SeedUserFour.user.Id, // Using centralized user ID
                    ScopeID = scope.ScopeID
                };
            }
        }

        public static Guid ContentId = new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3");
        public string content =
            "Homelessness remains a pervasive and complex crisis affecting individuals, families, and entire communities " +
            "across urban and rural areas alike. Driven by a combination of factors—including unaffordable housing, poverty, " +
            "unemployment, mental health challenges, substance use disorders, and systemic inequality—homelessness not only " +
            "strips individuals of stability and dignity but also places strain on public services and local economies.\n\n" +

            "Marginalized populations, such as veterans, LGBTQ+ youth, people of color, and those exiting foster care or " +
            "incarceration, are disproportionately impacted. Despite numerous policy efforts, shelters remain overcrowded, " +
            "permanent housing solutions underfunded, and preventive measures insufficient.\n\n" +

            "Tackling homelessness requires a coordinated, compassionate approach that addresses both immediate needs and " +
            "the root causes of housing instability.";


        public Scope scope { get {
                return new Scope()
                {
                    ScopeID = new Guid("b2e2e2c7-7e2a-4e2d-9b1a-2c3e4f5a6b7c"),
                    Scales = { Scale.Community, Scale.Regional, Scale.National },
                    Domains = { Domain.Economic, Domain.Political, Domain.Cultural, Domain.Technological, Domain.Health },
                    EntityTypes = { EntityType.Person, EntityType.Organization, EntityType.Government },
                    Boundaries = { BoundaryType.Jurisdictional, BoundaryType.Social },
                    Timeframes = { Timeframe.Generational }
                };
            }
        }

        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 18),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 19),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 20),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 1, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 1, 25),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 6,
                    CreatedAt = new DateTime(2024, 1, 26),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 27),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("1e2f3a4b-5c6d-7e8f-9a0b-1c2d3e4f5a6b"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 1, 28),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2f3a4b5c-6d7e-8f9a-0b1c-2d3e4f5a6b7c"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 1, 29),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3a4b5c6d-7e8f-9a0b-1c2d-3e4f5a6b7c8d"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 1, 30),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("4b5c6d7e-8f9a-0b1c-2d3e-4f5a6b7c8d9e"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 1, 31),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5c6d7e8f-9a0b-1c2d-3e4f-5a6b7c8d9e0f"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 2, 1),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("6d7e8f9a-0b1c-2d3e-4f5a-6b7c8d9e0f1a"),
                    UserID = SeedUserSixteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 2, 2),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7e8f9a0b-1c2d-3e4f-5a6b-7c8d9e0f1a2b"),
                    UserID = SeedUserSeventeen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 2, 3),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8f9a0b1c-2d3e-4f5a-6b7c-8d9e0f1a2b3c"),
                    UserID = SeedUserEighteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 2, 4),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("9a0b1c2d-3e4f-5a6b-7c8d-9e0f1a2b3c4d"),
                    UserID = SeedUserNineteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 2, 5),
               },
        };
    }
}
