using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
 
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions
{
    public class SeedSolutions
    {


     

        public static SeedSolutionContainer[] SeedSolutionDataContainers = {
             new MobileOutreachTeamsWithCliniciansAndSocialWorkers(),
             //new AtlasThePublicThinkTank(),
             //new TransparentAlgorithmSettings(),
             //new MoodBubbles(),
             //new ConsumptionTracker(),
             //new RoleplayThreads(),
             //new AnnotationMode(),
             //new PaidTransitionalEmployment(),
             //new SecondChanceHiringIncentivesForEmployers(),
             //new PublicEducationCampaignsToReduceStigma(),
             //new MicrograntsForUnhousedEntrepreneursOrGigWorkers(),
             //new ModularPosting(),
             //new MakeAlgorithmsUserAdjustable(),
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
