using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("fa86f462-e7ed-47b4-8c86-c6cc5bfc02e8"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("f7c6b5d9-2a48-47e3-83c1-a5b9d2e7f038"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), null, "Social media platforms should redesign their interaction systems to prioritize deliberative and civil discourse over confrontational exchanges that fuel polarization. By restructuring the fundamental ways users engage with political content and each other, platforms can create environments that reward thoughtful engagement rather than escalation and outrage.\n\nKey elements of this solution include:\n\n- Structured discussion formats that encourage thoughtful exchanges: Replace simple comment threads with frameworks that prompt users to identify points of agreement before expressing disagreement, articulate underlying values, and respond to specific aspects of others' arguments rather than engaging in sweeping dismissals\n\n- Expanded interaction options beyond binary reactions: Move beyond like/dislike buttons to include nuanced response options such as 'thoughtful point,' 'changed my perspective,' 'well-evidenced,' or 'respectfully disagree,' rewarding substance over mere emotional reactions\n\n- Cooling-off periods and reflection prompts: Introduce brief delays before publishing responses to heated political content, with optional reflection prompts asking users to consider whether their comment advances the conversation and how it might be received\n\n- Community recognition systems for bridge-building: Develop reputation systems that highlight and reward users who consistently engage constructively across political divides, elevating their contributions in discussions\n\n- Collaborative features that incentivize finding common ground: Create special formats for issues that encourage users from different viewpoints to collaboratively draft statements of shared principles or potential compromises\n\n- Friction for escalation patterns: Add increasing levels of friction (time delays, additional prompts) when conversation patterns show signs of unproductive escalation, without blocking communication entirely\n\nImplementation would require significant user experience research and iterative design, with transparent metrics tracking improvements in discourse quality. Platforms could introduce these features in opt-in communities initially, gradually expanding as positive outcomes are demonstrated.\n\nThis approach fundamentally changes incentive structures that currently reward divisiveness. By designing interaction systems that make thoughtful engagement easier and more satisfying than performative conflict, platforms can foster environments where users experience the genuine intellectual and social rewards of constructive political discourse rather than the hollow dopamine hits of tribal combat.", 1, new DateTime(2024, 5, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), new Guid("fa86f462-e7ed-47b4-8c86-c6cc5bfc02e8"), "Encourage Engagement, Not Escalation" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("f7c6b5d9-2a48-47e3-83c1-a5b9d2e7f038"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("fa86f462-e7ed-47b4-8c86-c6cc5bfc02e8"));
        }
    }
}
