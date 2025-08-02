using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class PaidTransitionalEmployment : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = Homelessness.ContentId,
                    Title = "Paid Transitional Employment Programs",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 10, 15),
                    AuthorID = SeedUserFive.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0");
        public string content =
            "Paid Transitional Employment programs offer structured, time-limited work opportunities that provide real income, " +
            "build skills, and create pathways to permanent employment for individuals experiencing homelessness. These " +
            "initiatives recognize that stable employment is a critical component of housing security, while acknowledging " +
            "that many unhoused individuals face significant barriers to entering the traditional workforce immediately.\n\n" +

            "Urban clean-up initiatives represent one successful model, employing homeless or recently housed individuals to " +
            "maintain public spaces, remove litter, abate graffiti, and beautify neighborhoods. These programs serve multiple " +
            "purposes: providing meaningful work with immediate compensation, improving community environments, fostering " +
            "positive interactions between homeless individuals and the broader community, and demonstrating participants' " +
            "capabilities and work ethic to potential employers.\n\n" +

            "Beyond urban clean-up, effective transitional employment models include: maintenance and restoration of public " +
            "parks and trails; peer outreach and navigation services for other homeless individuals; food service in community " +
            "kitchens; retail positions in social enterprise businesses; administrative support in nonprofit organizations; " +
            "and environmental stewardship projects. The most successful programs carefully match positions to participants' " +
            "existing skills and interests while providing opportunities to develop new capabilities.\n\n" +

            "Comprehensive programs incorporate several key elements: predictable schedules with flexible options to accommodate " +
            "health needs and service appointments; graduated responsibility as participants build confidence and skills; " +
            "regular compensation at fair wages, ideally with opportunities for wage progression; integrated support services " +
            "including case management, housing assistance, and mental health resources; financial literacy training and banking " +
            "access; job-readiness preparation such as resume building and interview skills; and explicit pathways to permanent " +
            "employment through partnerships with local businesses, preferential hiring agreements, or supported job placement.\n\n" +

            "Transitional employment initiatives require thoughtful design to avoid potential pitfalls such as creating " +
            "dependency or perpetuating low-wage work. Programs should establish clear timelines and goals, ensure that " +
            "participants receive genuine skill development rather than just busywork, maintain strong relationships with " +
            "permanent employers, and provide ongoing support during transitions to unsubsidized employment.\n\n" +

            "When properly implemented, paid transitional employment delivers significant returns on investment: participants " +
            "gain income stability, work experience, and self-confidence; communities benefit from improved public spaces and " +
            "reduced visible homelessness; employers access a prepared workforce; and public systems may realize cost savings " +
            "through reduced reliance on emergency services, shelters, and other crisis interventions.";

        public SolutionVote[] solutionVotes { get; } = {
             
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f4a0e9d8-1c65-40b3-b298-5e71f6a4b0c9"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 17),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a5b1f0e9-2d76-41c4-b309-6f82a7b5c1d0"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 18),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b6c2a1f0-3e87-42d5-b410-7a93b8c6d2e1"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 10, 19),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c7d3b2a1-4f98-43e6-a521-8b04c9d7e3f2"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 20),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d8e4c3b2-5a09-44f7-b632-9c15d0e8f4a3"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 21),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e9f5d4c3-6b10-45a8-b743-0d26e1f9a5b4"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 22),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f0a6e5d4-7c21-46b9-b854-1e37f2a0b6c5"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 23),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a1b7f6e5-8d32-47c0-b965-2f48a3b1c7d6"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 10, 24),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b2c8a7f6-9e43-48d1-b076-3a59b4c2d8e7"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 25),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c3d9b8a7-0f54-49e2-b187-4b60c5d3e9f8"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 26),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d4e0c9b8-1a65-40f3-b298-5c71d6e4f0a9"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 27),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e5f1d0c9-2b76-41a4-b309-6d82e7f5a1b0"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 28),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f6a2e1d0-3c87-42b5-b410-7e93f8a6b2c1"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 29),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a7b3f2e1-4d98-43c6-b521-8f04a9b7c3d2"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 10, 30),
               },
        };
    }
}