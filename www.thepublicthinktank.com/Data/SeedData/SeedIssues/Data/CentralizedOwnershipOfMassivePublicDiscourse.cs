using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data
{
    public class CentralizedOwnershipOfMassivePublicDiscourse : SeedIssueContainer
    {
        public Issue issue
        {
            get
            {
                return new Issue
                {
                    IssueID = ContentId,
                    Title = "Centralized Ownership of Massive Public Discourse",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 3, 20),
                    AuthorID = SeedUserSeven.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global, // Using centralized scope ID
                    ParentIssueID = CanSocialMediaPlatformsBeBetter.ContentId // Making it a sub-issue of the social media platforms issue
                };
            }
        }

        public static Guid ContentId = new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45");
        public string content =
            "The landscape of digital public discourse has become increasingly concentrated in the hands of a few powerful " +
            "technology companies. A small number of private corporations now own and control the primary platforms where " +
            "billions of people communicate, share information, and form opinions on matters of public importance. This " +
            "unprecedented centralization of communicative power raises profound questions for democratic societies.\n\n" +

            "Unlike traditional media which operated under various public interest obligations, social media platforms " +
            "function largely as private spaces governed by corporate terms of service rather than democratic principles. " +
            "This means that critical decisions about acceptable speech, content moderation, and algorithmic amplification " +
            "are made by executives accountable primarily to shareholders rather than citizens or elected representatives.\n\n" +

            "The consequences of this arrangement are far-reaching. Platform owners can unilaterally establish rules " +
            "affecting billions of users across diverse cultural and political contexts. They can amplify or suppress " +
            "certain types of content based on opaque algorithms. And they can implement sweeping policy changes with " +
            "minimal external oversight or transparent justification.\n\n" +

            "While these platforms have enabled unprecedented global connection and democratized content creation, the " +
            "consolidation of control over our primary communication infrastructure in so few hands poses significant " +
            "risks. Questions of platform monopoly power, alternative ownership models, and appropriate governance " +
            "frameworks have become urgent as digital communications increasingly shape our public life and democratic " +
            "processes. Finding the right balance between innovation, free expression, and democratic accountability " +
            "remains one of the central challenges of our digital age.";

        public IssueVote[] issueVotes { get; } = {
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a7b8c9d0-1e2f-4a3b-5c6d-7e8f9a0b1c2d"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 3, 21),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b8c9d0e1-2f3a-4b5c-6d7e-8f9a0b1c2d3e"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 3, 22),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c9d0e1f2-3a4b-4c5d-7e8f-9a0b1c2d3e4f"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 3, 23),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d0e1f2a3-4b5c-4d6e-8f9a-0b1c2d3e4f5a"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 3, 24),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e1f2a3b4-5c6d-4e7f-9a0b-1c2d3e4f5a6b"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 3, 25),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f2a3b4c5-6d7e-4f8a-0b1c-2d3e4f5a6b7c"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 3, 26),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a3b4c5d6-7e8f-4a9b-1c2d-3e4f5a6b7c8d"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 3, 27),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b4c5d6e7-8f9a-4b0c-2d3e-4f5a6b7c8d9e"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 3, 28),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c5d6e7f8-9a0b-4c1d-3e4f-5a6b7c8d9e0f"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 3, 29),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("d6e7f8a9-0b1c-4d2e-4f5a-6b7c8d9e0f1a"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 7,
                    CreatedAt = new DateTime(2024, 3, 30),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("e7f8a9b0-1c2d-4e3f-5a6b-7c8d9e0f1a2b"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 3, 31),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("f8a9b0c1-2d3e-4f4a-6b7c-8d9e0f1a2b3c"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 4, 1),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("a9b0c1d2-3e4f-4a5b-7c8d-9e0f1a2b3c4d"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 4, 2),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("b0c1d2e3-4f5a-4b6c-8d9e-0f1a2b3c4d5e"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 4, 3),
               },
               new IssueVote(){
                    IssueID = ContentId,
                    VoteID = new Guid("c1d2e3f4-5a6b-4c7d-9e0f-1a2b3c4d5e6f"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 4, 4),
               },
        };
    }
}
