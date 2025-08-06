using repository_pattern_experiment.Data.SeedData.SeedIssues.Data;
using repository_pattern_experiment.Data.SeedData.SeedUsers.Data;
using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.SeedData.SeedSolutions.Data
{
    public class MicrograntsForUnhousedEntrepreneursOrGigWorkers : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = Homelessness.ContentId,
                    Title = "Microgrants for Unhoused Entrepreneurs or Gig Workers",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 9, 15),
                    AuthorID = SeedUserNine.user.Id, // Using centralized user ID
                    ScopeID = SeedIds.Scopes.Global // Using centralized scope ID
                };
            }
        }

        public static Guid ContentId = new Guid("c5d2e3f2-b1a0-47c9-8d67-5f2e3b4a1d90");
        public string content =
            "Microgrants for Unhoused Entrepreneurs or Gig Workers represents an innovative approach to addressing homelessness " +
            "by fostering economic self-sufficiency through small-scale entrepreneurship and gig economy participation. This " +
            "solution recognizes that many individuals experiencing homelessness possess valuable skills, creativity, and drive " +
            "that can be leveraged to generate income when traditional employment paths may be inaccessible.\n\n" +

            "The program provides modest financial grants (typically $500-$5,000) directly to qualifying unhoused individuals " +
            "with viable business ideas or who need equipment and resources to participate in gig economy opportunities. These " +
            "funds can be used for essential business expenses: purchasing equipment or tools, securing necessary licenses or " +
            "certifications, accessing digital technology, acquiring inventory, or covering transportation costs to work sites.\n\n" +

            "Beyond financial support, a comprehensive microgrant program includes complementary resources that maximize success " +
            "potential: business skills training tailored to different entrepreneurial ventures; mentorship from established " +
            "entrepreneurs in similar fields; assistance with digital literacy and online platform navigation; simplified " +
            "accounting tools for financial management; and connections to community markets, online platforms, or local " +
            "businesses where goods or services can be sold.\n\n" +

            "Implementation requires thoughtful design: low-barrier application processes that don't exclude those without " +
            "formal documentation; tiered funding levels that allow for growth as businesses develop; flexibility in eligible " +
            "expenses to accommodate diverse business models; and staged disbursement tied to business milestones rather than " +
            "rigid timelines.\n\n" +

            "The benefits extend beyond immediate income generation. Participants develop transferable skills, build credit " +
            "and financial history, establish professional networks, and gain confidence and dignity through meaningful work. " +
            "The resulting financial stability helps secure and maintain housing, while successful microenterprises can " +
            "potentially grow to employ others facing similar circumstances, creating a positive ripple effect in communities.";

        public SolutionVote[] solutionVotes { get; } = {
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d6e7f8a9-b0c1-47d2-93e4-5f8c7b6a9d0e"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 9, 16),
               }
        };
    }
}