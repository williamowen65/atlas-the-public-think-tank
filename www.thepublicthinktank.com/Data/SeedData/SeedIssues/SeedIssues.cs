using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Models.Database;
using Microsoft.EntityFrameworkCore;
using System;

namespace atlas_the_public_think_tank.Data.SeedData.SeedIssues
{
    public class SeedIssues
    {
        public static SeedIssueContainer[] SeedIssuesDataContainers = {
                    new ClimateChange(),
                    new CriticalDeclineOfEndangeredSpecies(),
                    new Homelessness(),
                    new AffordableHousing(),
                    new CanSocialMediaPlatformsBeBetter(),
                    new DeclineOfSouthernResidentOrcaPopulation(),
                    new OceanAcidification(),
                    new WorkforceAutomationAndJobDisplacement(),
                    new ImpactOnAdolescentMentalHealthAndBodyImage(),
                    new AdDrivenModelsIncentivizingOutrageAndEngagementAtAllCosts(),
                    new CentralizedOwnershipOfMassivePublicDiscourse(),
                    new SpreadOfMisinformationAndEchoChambers(),
                    new AmplificationOfPoliticalPolarizationAndExtremism(),
                    new CensorshipVsFreeSpeechTensions(),
                    new StigmaPreventingPeopleFromSeekingHelp(),
                    new HousingSupplyAndAffordability(),
                    new SystemicFailuresAndSafetyNets(),
                    new GapsInTransitionalServicesAfterFosterCarePrisonOrMilitaryService(),
                    new DiscoverabilityAndVisibilityOfContributions(),
                    new MisinformationAndBadFaithParticipation(),
                    new IntellectualPropertyAndAttribution(),
                    new ModerationAndGovernanceOfPublicDebates(),
                    new BiasAndRepresentationInParticipation(),
                    new SustainingLongTermEngagement(),
                    new BalancingTransparencyWithAnonymity(),
                    new TranslationAndGlobalAccessibility()
            };

        public static Issue[] SeedIssuesData = SeedIssuesDataContainers
          .Select(container => container.issue) 
          .ToArray();

        public SeedIssues(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Issue>().HasData(SeedIssuesData);
        }
    }


    public interface SeedIssueContainer
    {
        Issue issue { get; }
        IssueVote[] issueVotes { get; }
    }

}