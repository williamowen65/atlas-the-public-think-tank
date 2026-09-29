using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
 
using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data
{
    public class MobileOutreachTeamsWithCliniciansAndSocialWorkers : SeedSolutionContainer
    {
        public Solution solution
        {
            get
            {
                return new Solution
                {
                    SolutionID = ContentId,
                    ParentIssueID = Homelessness.ContentId,
                    Title = "Mobile Outreach Teams with Clinicians and Social Workers",
                    Content = content,
                    ContentStatus = ContentStatus.Published,
                    CreatedAt = new DateTime(2024, 10, 25),
                    AuthorID = SeedUserThree.user.Id, // Using centralized user ID
                    ScopeID = scope.ScopeID // Using centralized scope ID
                };
            }
        }


        public Scope scope
        {
            get
            {
                return new Scope()
                {
                    ScopeID = new Guid("96bb172c-23fc-4359-b324-e221db3682a9"),
                    Scales = { Scale.Community },
                    Domains = { Domain.Environmental },
                    EntityTypes = { EntityType.Organization, EntityType.Government, EntityType.Person },
                    Boundaries = { BoundaryType.Social, BoundaryType.Jurisdictional },
                    Timeframes = { Timeframe.Immediate, Timeframe.Generational }
                };
            }
        }



        public static Guid ContentId = new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1");
        public string content =
            "Mobile Outreach Teams with Clinicians and Social Workers represent a proactive, relationship-based approach to " +
            "engaging people experiencing homelessness who may be disconnected from traditional service systems. By bringing " +
            "multidisciplinary expertise directly to individuals where they live—whether in encampments, vehicles, abandoned " +
            "buildings, or other unsheltered locations—these teams establish trust, provide immediate assistance, and create " +
            "pathways to housing, healthcare, and long-term support.\n\n" +

            "Effective mobile outreach teams typically include several key professionals working in coordination: Licensed " +
            "clinicians (psychiatrists, psychiatric nurse practitioners, or clinical social workers) who can conduct field-based " +
            "mental health and substance use assessments, provide brief interventions, prescribe medications when appropriate, " +
            "and facilitate connections to ongoing treatment; Social workers or case managers who assist with benefits applications, " +
            "housing navigation, and coordination of various services; Peer support specialists with lived experience of homelessness " +
            "who offer authentic connection, practical guidance, and hope through their own recovery journeys; and occasionally, " +
            "specially trained law enforcement officers or emergency medical technicians who can address safety concerns or " +
            "medical emergencies with a humanitarian, rather than punitive, approach.\n\n" +

            "The operational model emphasizes consistency, persistence, and respect for individual autonomy. Teams visit the same " +
            "locations on predictable schedules, allowing for relationship development over time. They practice trauma-informed " +
            "engagement, recognizing that many homeless individuals have experienced past traumatic events, including negative " +
            "interactions with service systems. Rather than requiring immediate compliance with program expectations, teams work " +
            "at the individual's pace, beginning with low-barrier assistance that addresses immediate needs—food, hygiene supplies, " +
            "wound care, harm reduction supplies—while gradually building trust for more intensive interventions.\n\n" +

            "Mobile outreach teams are equipped with technology and resources that enable field-based service delivery: Tablets " +
            "or laptops with cellular connectivity for real-time documentation, benefits applications, and housing registries; " +
            "Transportation capacity to accompany clients to appointments; Basic medical supplies for first aid and health " +
            "assessments; Emergency funds for immediate needs like temporary accommodations or identification documents; and " +
            "Direct access to shelter beds or transitional housing units reserved specifically for outreach referrals, allowing " +
            "teams to offer immediate alternatives to unsheltered homelessness.\n\n" +

            "When implemented effectively, mobile outreach yields significant benefits: Improved engagement of highly vulnerable " +
            "individuals who would otherwise remain disconnected from services; Reduced reliance on costly emergency systems like " +
            "hospitals and jails; Earlier intervention in health and mental health conditions before they reach crisis levels; " +
            "More successful housing placements due to the trust established through consistent outreach; and Improved community " +
            "relations by addressing visible homelessness with compassion rather than criminalization.\n\n" +

            "Successful implementation requires dedicated funding for competitive salaries, appropriate staffing ratios, quality " +
            "supervision, and comprehensive training. Programs must balance geographic coverage with sufficient time for meaningful " +
            "engagement, avoid becoming merely a crisis response system, and maintain strong connections to housing resources to " +
            "ensure outreach leads to permanent solutions rather than merely managing homelessness.";

        public SolutionVote[] solutionVotes { get; } = {
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f0e9d8c7-6b53-40a4-b298-3d71e6f4a0b9"),
                    UserID = SeedUserOne.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 10, 26),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a1f0e9d8-7c64-41b5-b309-4e82f7a5b1c0"),
                    UserID = SeedUserTwo.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 27),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b2a1f0e9-8d75-42c6-b410-5f93a8b6c2d1"),
                    UserID = SeedUserThree.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 10, 28),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c3b2a1f0-9e86-43d7-b521-6a04b9c7d3e2"),
                    UserID = SeedUserFour.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 29),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d4c3b2a1-0f97-44e8-a632-7b15c0d8e4f3"),
                    UserID = SeedUserFive.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 10, 30),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e5d4c3b2-1a08-45f9-b743-8c26d1e9f5a4"),
                    UserID = SeedUserSix.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 10, 31),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f6e5d4c3-2b19-46a0-b854-9d37e2f0a6b5"),
                    UserID = SeedUserSeven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 11, 1),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a7f6e5d4-3c20-47b1-b965-0e48f3a1b7c6"),
                    UserID = SeedUserEight.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 11, 2),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b8a7f6e5-4d31-48c2-b076-1f59a4b2c8d7"),
                    UserID = SeedUserNine.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 11, 3),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("c9b8a7f6-5e42-49d3-b187-2a60b5c3d9e8"),
                    UserID = SeedUserTen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 11, 4),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("d0c9b8a7-6f53-40e4-b298-3b71c6d4e0f9"),
                    UserID = SeedUserEleven.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 11, 5),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("e1d0c9b8-7a64-41f5-b309-4c82d7e5f1a0"),
                    UserID = SeedUserTwelve.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 11, 6),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("f2e1d0c9-8b75-42a6-b410-5d93e8f6a2b1"),
                    UserID = SeedUserThirteen.user.Id,
                    VoteValue = 8,
                    CreatedAt = new DateTime(2024, 11, 7),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("a3f2e1d0-9c86-43b7-b521-6e04f9a7b3c2"),
                    UserID = SeedUserFourteen.user.Id,
                    VoteValue = 9,
                    CreatedAt = new DateTime(2024, 11, 8),
               },
               new SolutionVote(){
                    SolutionID = ContentId,
                    VoteID = new Guid("b4a3f2e1-0d97-44c8-b632-7f15a0b8c4d3"),
                    UserID = SeedUserFifteen.user.Id,
                    VoteValue = 10,
                    CreatedAt = new DateTime(2024, 11, 9),
               },
        };
    }
}