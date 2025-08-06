using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.SeedData.SeedIssues.Data
{
    public class AmplificationOfPoliticalPolarizationAndExtremism : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Amplification of Political Polarization and Extremism",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 4, 25),
                    AuthorID = SeedUserSix.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId // Making it a sub-issue of the social media platforms issue
                };
            }
        }

        public static Guid ContentId = new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8");
        public string content =
            "Social media platforms have fundamentally altered how political discourse unfolds, often intensifying political " +
            "divisions and creating environments where extremist viewpoints can flourish. Several structural elements of these " +
            "platforms contribute to this phenomenon, presenting challenges for democratic societies globally.\n\n" +

            "Recommendation algorithms typically prioritize content that generates strong emotional reactions, including " +
            "outrage and partisan anger. This creates feedback loops where increasingly extreme political content receives " +
            "greater visibility and engagement, effectively rewarding polarization. Meanwhile, platform architecture often " +
            "facilitates the formation of ideologically homogeneous communities where more moderate voices are marginalized " +
            "and radical ideas become normalized through group dynamics and reinforcement.\n\n" +

            "The attention economy of these platforms also incentivizes politicians, media outlets, and content creators " +
            "to adopt more extreme, divisive positions to maintain visibility and audience engagement. Complex policy " +
            "discussions are reduced to inflammatory sound bites, and nuanced perspectives struggle to gain traction " +
            "in an environment optimized for controversy rather than understanding.\n\n" +

            "Additionally, malicious actors—including some foreign governments—have exploited these platform vulnerabilities " +
            "to intentionally amplify existing social divisions, often using sophisticated targeting techniques to reach " +
            "receptive audiences with content designed to heighten tensions and undermine democratic discourse.\n\n" +

            "Addressing these challenges requires examining the design choices that facilitate polarization and extremism, " +
            "exploring alternative platform architectures that might foster healthier political discourse, and developing " +
            "literacy around how these systems shape our understanding of political issues. Solutions must balance " +
            "concerns about censorship and free expression against the need for information environments that support " +
            "democratic values rather than undermine them.";

        public IssueVote[] issueVotes { get; } = {
        
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7b8c9d0e-1f2a-43b4-c5d6-7e8f9a0b1c2d"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 4, 27),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8c9d0e1f-2a3b-44c5-d6e7-8f9a0b1c2d3e"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 4, 28),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("9d0e1f2a-3b4c-45d6-e7f8-9a0b1c2d3e4f"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 4, 29),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("0e1f2a3b-4c5d-46e7-f8a9-0b1c2d3e4f5a"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 4, 30),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("1f2a3b4c-5d6e-47f8-a90b-1c2d3e4f5a6b"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 1),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2a3b4c5d-6e7f-48a9-0b1c-2d3e4f5a6b7c"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 2),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3b4c5d6e-7f8a-490b-1c2d-3e4f5a6b7c8d"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 3),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("4c5d6e7f-8a9b-400c-2d3e-4f5a6b7c8d9e"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 4),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5d6e7f8a-9b0c-41d2-3e4f-5a6b7c8d9e0f"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 5, 5),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("6e7f8a9b-0c1d-42e3-4f5a-6b7c8d9e0f1a"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 6),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7f8a9b0c-1d2e-43f4-5a6b-7c8d9e0f1a2b"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 5, 7),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8a9b0c1d-2e3f-44a5-6b7c-8d9e0f1a2b3c"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 8),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("9b0c1d2e-3f4a-45b6-7c8d-9e0f1a2b3c4d"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 5, 9),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("0c1d2e3f-4a5b-46c7-8d9e-0f1a2b3c4d5e"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 5, 10),
               },
        };
    }
}
