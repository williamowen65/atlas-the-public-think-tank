using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions
{
    public class SeedSolutions
    {


     

        public static SeedSolutionContainer[] SeedSolutionDataContainers = {
             new AtlasThePublicThinkTank(),
             new TransparentAlgorithmSettings(),
             new MoodBubbles(),
             new ConsumptionTracker(),
             new RoleplayThreads(),
             new AnnotationMode(),
             new ModularPosting(),
             new SecondChanceHiringIncentivesForEmployers(),
             new PublicEducationCampaignsToReduceStigma(),
             new PaidTransitionalEmployment(),
             new MobileOutreachTeamsWithCliniciansAndSocialWorkers(),
             new MicrograntsForUnhousedEntrepreneursOrGigWorkers(),
             new MakeAlgorithmsUserAdjustable(),
             //new DownRankPersonalAttacksAndPerformativeOutrage()
         };

        public static Solution[] SeedSolutionData = SeedSolutionDataContainers
          .Select(container => container.solution)
          .ToArray();

        public SeedSolutions(ModelBuilder modelBuilder)
        {
            // Seed Solutions
            modelBuilder.Entity<Solution>().HasData(SeedSolutionData);
        }
    }

    public interface SeedSolutionContainer
    {
        Solution solution { get; }
        SolutionVote[] solutionVotes { get; }
    }
}
