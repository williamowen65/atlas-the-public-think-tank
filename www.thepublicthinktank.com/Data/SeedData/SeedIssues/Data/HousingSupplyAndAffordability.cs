using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class HousingSupplyAndAffordability : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Housing Supply and Affordability Crisis",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 7, 15),
                    AuthorID = SeedUserThree.user.Id, // Using centralized user ID
                    ScopeID = scope.ScopeID,
                    ParentIssueID = Homelessness.ContentId
                };
            }
        }

        public static Guid ContentId = new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930");
        public string content =
            "Housing markets across many regions are experiencing a profound and multifaceted crisis that extends far beyond " +
            "homelessness to affect middle-income households, young adults, retirees, and virtually all segments of society. " +
            "This crisis manifests in rapidly escalating home prices and rents that consistently outpace wage growth, creating " +
            "a situation where housing costs consume an unsustainable portion of household incomes.\n\n" +

            "At the heart of this issue lies a fundamental supply-demand imbalance. Decades of underbuilding have resulted " +
            "in housing shortages estimated in the millions of units. This undersupply stems from multiple factors working " +
            "in tandem: restrictive zoning laws that prevent density and efficient land use; complex and lengthy approval " +
            "processes that increase development costs and timelines; construction labor shortages; rising material costs; " +
            "and significant barriers to scaling innovative building technologies.\n\n" +

            "The consequences of this crisis extend beyond housing itself. Economic mobility is hampered as workers cannot " +
            "afford to live near job opportunities. Intergenerational wealth gaps widen as homeownership—historically a " +
            "primary vehicle for middle-class wealth building—becomes increasingly inaccessible to younger generations. " +
            "Environmental goals suffer as housing shortages in transit-rich urban areas push development to car-dependent " +
            "exurbs, increasing commute times and carbon emissions.\n\n" +

            "Communities face additional challenges as essential workers—teachers, healthcare providers, first responders—" +
            "are priced out of the areas they serve. Demographic shifts occur as families delay formation, aging adults " +
            "cannot downsize appropriately, and diverse populations are displaced from established neighborhoods.\n\n" +

            "Addressing this crisis requires coordinated efforts across multiple domains: land use reform to enable more " +
            "housing production of varied types; investment in housing subsidies and affordable development; innovations " +
            "in construction methods and financing models; tenant protections that maintain stability without discouraging " +
            "supply growth; and regional approaches that recognize housing markets transcend municipal boundaries.";

        public Scope scope
        {
            get
            {
                return new Scope()
                {
                    ScopeID = new Guid("d34e087c-ef33-4dd1-98fd-759440c16961"),
                    Scales = { Scale.National, Scale.Community },
                    Domains = { Domain.Economic, Domain.Health, Domain.Environmental },
                    EntityTypes = { EntityType.Government, EntityType.Organization },
                    Timeframes = { Timeframe.LongTerm },
                    Boundaries = { },
                };
            }
        }
        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a7b6c5d4-e3f2-41a0-b9c8-d7e6f5a4b3c2"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 7, 16),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b8c7d6e5-f4a3-42b1-c9d0-e8f7a6b5c4d3"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 7, 17),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c9d8e7f6-a5b4-43c2-d0e1-f9a8b7c6d5e4"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 7, 18),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d0e9f8a7-b6c5-44d3-e1f2-a0b9c8d7e6f5"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 7, 19),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e1f0a9b8-c7d6-45e4-f2a3-b1c0d9e8f7a6"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 7, 20),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f2a1b0c9-d8e7-46f5-a3b4-c2d1e0f9a8b7"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 7, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a3b2c1d0-e9f8-47a6-b5c4-d3e2f1a0b9c8"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 7, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b4c3d2e1-f0a9-48b7-c6d5-e4f3a2b1c0d9"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 7, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c5d4e3f2-a1b0-49c8-d7e6-f5a4b3c2d1e0"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 7, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d6e5f4a3-b2c1-40d9-e8f7-a6b5c4d3e2f1"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 7, 25),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e7f6a5b4-c3d2-41e0-f9a8-b7c6d5e4f3a2"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 7, 26),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f8a7b6c5-d4e3-42f1-a0b9-c8d7e6f5a4b3"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 7, 27),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a9b8c7d6-e5f4-43a2-b1c0-d9e8f7a6b5c4"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 7, 28),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b0c9d8e7-f6a5-44b3-c2d1-e0f9a8b7c6d5"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 7, 29),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c1d0e9f8-a7b6-45c4-d3e2-f1a0b9c8d7e6"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 7, 30),
               },
        };
    }
}
