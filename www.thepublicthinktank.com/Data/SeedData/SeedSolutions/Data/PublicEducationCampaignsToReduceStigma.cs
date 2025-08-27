using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class PublicEducationCampaignsToReduceStigma : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = StigmaPreventingPeopleFromSeekingHelp.ContentId,
                    Title = "Public Education Campaigns to Reduce Stigma",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 10, 5),
                    AuthorID = SeedUserSeven.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d");
        public string content =
            "Public Education Campaigns to Reduce Stigma represents a strategic approach to changing societal perceptions and " +
            "attitudes about homelessness through coordinated, evidence-based messaging and community engagement. By tackling " +
            "misconceptions and humanizing the experience of housing instability, these campaigns can help dismantle one of the " +
            "most significant barriers preventing people from seeking assistance.\n\n" +

            "Effective stigma reduction campaigns are multi-faceted, employing various communication channels and approaches. " +
            "Mass media components utilize billboards, public service announcements, social media campaigns, and traditional " +
            "advertising to challenge stereotypes and present accurate information about the causes of homelessness, emphasizing " +
            "structural factors like housing affordability, economic instability, and insufficient support systems rather than " +
            "personal failings. These campaigns feature authentic stories and images that highlight the diversity of people " +
            "experiencing homelessness, avoiding sensationalism while preserving dignity.\n\n" +

            "Community engagement initiatives complement mass media efforts through in-person educational workshops, speaking " +
            "engagements at schools and community organizations, interactive exhibits, and public forums where housed and unhoused " +
            "community members can engage in facilitated dialogue. These face-to-face interactions help build empathy by creating " +
            "spaces for genuine connection and understanding.\n\n" +

            "Peer ambassador programs represent a particularly powerful component, training and employing individuals with lived " +
            "experience of homelessness to serve as public speakers, media spokespeople, and community educators. This approach " +
            "not only provides authentic representation but also creates meaningful employment opportunities and recognition of " +
            "expertise gained through experience.\n\n" +

            "Targeted professional education reaches service providers, healthcare workers, law enforcement, educators, and other " +
            "professionals who regularly interact with people experiencing homelessness. This specialized training addresses " +
            "unconscious bias, promotes trauma-informed approaches, and provides practical strategies for creating more welcoming " +
            "and dignified service environments.\n\n" +

            "When implemented comprehensively and sustained over time, public education campaigns contribute to measurable shifts " +
            "in public attitudes, increased support for evidence-based solutions to homelessness, reduced discrimination in service " +
            "settings, and—most importantly—greater willingness among people experiencing homelessness to seek and engage with " +
            "available support services.";

        public SolutionVote[] solutionVotes { get; } = {
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b8e0d9c7-5f4a-48b3-a2c1-7d6e5f4a3b2c"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 6),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c9f1e0d8-6a5b-49c4-b3d2-8e7f6a5b4c3d"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 7),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d0a2f1e9-7b6c-40d5-b4e3-9f8a7b6c5d4e"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 10, 8),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e1b3a2f0-8c7d-41e6-b5f4-0a9b8c7d6e5f"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 9),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f2c4b3a1-9d8e-42f7-b6a5-1b0c9d8e7f6a"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 10),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a3d5c4b2-0e9f-43a8-b7b6-2c1d0e9f8a7b"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 11),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b4e6d5c3-1f0a-44b9-a8c7-3d2e1f0a9b8c"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 10, 12),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c5f7e6d4-2a1b-45c0-b9d8-4e3f2a1b0c9d"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 13),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d6a8f7e5-3b2c-46d1-b0e9-5f4a3b2c1d0e"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 14),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e7b9a8f6-4c3d-47e2-b1f0-6a5b4c3d2e1f"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 15),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f8c0b9a7-5d4e-48f3-b2a1-7b6c5d4e3f2a"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 10, 16),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a9d1c0b8-6e5f-49a4-b3b2-8c7d6e5f4a3b"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 17),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b0e2d1c9-7f6a-40b5-b4c3-9d8e7f6a5b4c"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 18),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c1f3e2d0-8a7b-41c6-b5d4-0e9f8a7b6c5d"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 19),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d2a4f3e1-9b8c-42d7-b6e5-1f0a9b8c7d6e"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 20),
               },
        };
    }
}