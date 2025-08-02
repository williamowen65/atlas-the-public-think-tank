using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class ImpactOnAdolescentMentalHealthAndBodyImage : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Impact on Adolescent Mental Health and Body Image",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 2, 15),
                    AuthorID = SeedUserThree.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId
                };
            }
        }

        public static Guid ContentId = new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12");
        public string content =
            "The rise of social media platforms has coincided with alarming trends in adolescent mental health and body image " +
            "concerns. Today's teenagers are exposed to a constant stream of carefully curated, often digitally altered images " +
            "that present unrealistic standards of beauty, success, and lifestyle. This digital environment has created " +
            "unprecedented challenges for young people developing their sense of self and place in the world.\n\n" +

            "Research increasingly suggests connections between heavy social media use and increased rates of depression, " +
            "anxiety, and body dissatisfaction among adolescents. The pressure to receive validation through likes and " +
            "comments, constant comparison to peers and influencers, and the fear of missing out (FOMO) can create harmful " +
            "psychological patterns that may persist into adulthood. Young women and LGBTQ+ youth appear particularly " +
            "vulnerable to these negative effects.\n\n" +

            "The algorithmic amplification of content that drives engagement often prioritizes extreme, idealized, or " +
            "controversial material, creating distorted perceptions of reality. Beauty filters and editing tools that alter " +
            "appearances have become normalized, blurring the line between authentic and manufactured self-presentation.\n\n" +

            "While social media platforms implement some safeguards, many argue these measures remain insufficient against " +
            "the powerful commercial incentives driving user engagement. Addressing this issue requires coordinated efforts " +
            "from technology companies, parents, educators, healthcare providers, and policymakers to create healthier " +
            "digital environments that support rather than undermine adolescent development and well-being.";

        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7a4b3c2d-1e5f-4a6b-8c9d-0e1f2a3b4c5d"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 2, 16),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8b5c4d3e-2f6a-4b7c-9d0e-1f2a3b4c5d6e"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 2, 17),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("9c6d5e4f-3a7b-4c8d-0e1f-2a3b4c5d6e7f"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 2, 18),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("0d7e6f5a-4b8c-4d9e-1f2a-3b4c5d6e7f8a"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 2, 19),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("1e8f7a6b-5c9d-4e0f-2a3b-4c5d6e7f8a9b"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 2, 20),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("2f9a8b7c-6d0e-4f1a-3b4c-5d6e7f8a9b0c"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 2, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("3a0b9c8d-7e1f-4a2b-4c5d-6e7f8a9b0c1d"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 2, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("4b1c0d9e-8f2a-4b3c-5d6e-7f8a9b0c1d2e"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 2, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("5c2d1e0f-9a3b-4c4d-6e7f-8a9b0c1d2e3f"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 2, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("6d3e2f1a-0b4c-4d5e-7f8a-9b0c1d2e3f4a"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 2, 25),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("7e4f3a2b-1c5d-4e6f-8a9b-0c1d2e3f4a5b"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 2, 26),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("8f5a4b3c-2d6e-4f7a-9b0c-1d2e3f4a5b6c"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 2, 27),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("9a6b5c4d-3e7f-4a8b-0c1d-2e3f4a5b6c7d"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 2, 28),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("0b7c6d5e-4f8a-4b9c-1d2e-3f4a5b6c7d8e"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 2, 29),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("1c8d7e6f-5a9b-4c0d-2e3f-4a5b6c7d8e9f"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 3, 1),
               },
        };
    }
}
