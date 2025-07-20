using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class LimitedAccessToEducationOrVocationalTraining : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Limited Access to Education or Vocational Training",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 9, 5),
                    AuthorID = SeedUserThirteen.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = GapsInTransitionalServicesAfterFosterCarePrisonOrMilitaryService.ContentId // Making this a sub-issue of GapsInTransitionalServicesAfterFosterCarePrisonOrMilitaryService
                };
            }
        }

        public static Guid ContentId = new Guid("1c5a7b8d-93e2-47f0-bd61-42a9c8f75d30");
        public string content =
            "Limited access to education and vocational training represents a significant barrier for individuals transitioning " +
            "from foster care, prison, or military service. Without these crucial opportunities for skill development and " +
            "credential attainment, many struggle to secure stable employment, achieve financial independence, and avoid " +
            "homelessness.\n\n" +

            "Former foster youth often lack the financial resources, guidance, and support networks needed to pursue higher " +
            "education or vocational training. Despite tuition waiver programs in some states, many still face challenges with " +
            "living expenses, academic preparation, and navigating complex educational systems without family support.\n\n" +

            "Formerly incarcerated individuals encounter significant barriers to educational and vocational programs, including " +
            "explicit exclusions from financial aid, licensing restrictions in many professions, and employment discrimination. " +
            "While in-prison educational programs show promise in reducing recidivism, they are often underfunded, inconsistent " +
            "in quality, and limited in scope.\n\n" +

            "Veterans may struggle to translate their military skills into civilian credentials or may face challenges adapting " +
            "to traditional educational environments due to physical or psychological injuries, family responsibilities, or " +
            "reintegration challenges. Despite the GI Bill, many veterans find it difficult to navigate educational benefits " +
            "or access programs that accommodate their unique needs.\n\n" +

            "Addressing these barriers requires comprehensive approaches: expanding financial support beyond tuition, providing " +
            "wraparound services including housing and childcare, developing trauma-informed educational environments, creating " +
            "flexible program structures, and fostering partnerships between educational institutions, employers, and social " +
            "service agencies. By improving access to quality education and training, we can help vulnerable individuals build " +
            "sustainable paths to stability and self-sufficiency.";

        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7d24c903-5a8f-48b2-ae17-0d96f582c4b1"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 6),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("61e8f295-0d7c-4ba3-94d1-f85c02a3e476"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 7),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c294f7a5-3db1-48e6-b07c-925ea1d4f683"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 9, 8),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f82d5c9a-4e71-46b0-83a9-02c7e943d5b1"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 9),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("053df786-e219-4bc8-a91e-37c5d8fe42a0"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 10),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b92c47e8-6a3d-45f1-80d9-5c197a4e3f28"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 9, 11),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("408a3df2-1e65-47bc-9d07-fe283c5ba194"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 12),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d5e79c0f-6b82-47a1-935d-2c48e07f51b0"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 13),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("392f7e8a-0d61-45b9-c87f-1e29a3d568c4"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 9, 14),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8f61c2d9-7e5a-48b3-90a4-1d73c6e95b20"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 15),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2c79e8f5-1a3b-46d0-97c5-0eb4a87d2f31"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 16),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a9b7c6d5-8e3f-42a1-b90d-7c58f1e2d430"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 9, 17),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5f2e8d7c-6b1a-43f9-9e05-8d71c2b34a96"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 9, 18),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c8d7e6f5-9a3b-41c2-8d0f-5e4a3b2c1d09"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 19),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("17f8e9d0-4c5b-46a2-8397-2e1d0c9b8a57"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 9, 20),
               },
        };
    }
}