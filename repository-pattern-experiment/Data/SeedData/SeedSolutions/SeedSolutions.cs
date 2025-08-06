using repository_pattern_experiment.Data.SeedData.SeedIssues;
using repository_pattern_experiment.Data.SeedData.SeedIssues.Data;
using repository_pattern_experiment.Data.SeedData.SeedSolutions.Data;
using repository_pattern_experiment.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace repository_pattern_experiment.Data.SeedData.SeedSolutions
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
