using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("56c24d9d-a605-4ed5-991e-cac7944cd747"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), null, "Social media platforms should implement content ranking systems that algorithmically de-prioritize personal attacks and performative outrage while elevating substantive political discourse. This solution addresses a core driver of political polarization: the current algorithmic preference for emotionally charged, divisive content over reasoned discussion.\n\nThe approach would involve several key components:\n\n- Natural language processing systems trained to distinguish between substantive political arguments and content that primarily consists of character attacks, inflammatory rhetoric, or performative moral outrage\n\n- Algorithmic adjustments that reduce the visibility of posts containing high levels of personal attacks or outrage-baiting language in feeds and recommendation systems\n\n- Corresponding promotion of content that addresses political topics with substantive arguments, evidence, and respectful engagement with opposing viewpoints\n\n- Transparency metrics showing users the percentage of 'high substance' versus 'high outrage' content in their feeds, with optional tools to further adjust these ratios\n\n- Regular public reporting on platform-wide trends in discourse quality and the effectiveness of ranking interventions\n\nImplementation would require careful design to avoid political bias, with regular auditing by diverse stakeholders to ensure the system doesn't inadvertently suppress legitimate political speech. Crucially, this approach doesn't remove or censor any content—it simply adjusts visibility based on discourse quality rather than engagement potential.\n\nThe benefits would be substantial: reduced amplification of extremist rhetoric, decreased incentives for politicians and media outlets to engage in inflammatory messaging, and the creation of social media environments more conducive to constructive political discourse. By shifting algorithmic incentives away from outrage and toward substance, platforms can help reverse the polarization cycle while still preserving a diverse range of political viewpoints.", 1, new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), new Guid("56c24d9d-a605-4ed5-991e-cac7944cd747"), "Down-Rank Personal Attacks and Performative Outrage" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a0f3e2d1-5c8b-40a9-c4b6-d7e0f9a8c3b2"), null, new DateTime(2024, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("a6f9e8d7-1c4b-46a5-c0b2-d3e6f5a4c9b8"), null, new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 8 },
                    { new Guid("b1a4f3e2-6d9c-41b0-d5c7-e8f1a0b9d4c3"), null, new DateTime(2024, 5, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("b7a0f9e8-2d5c-47b6-d1c3-e4f7a6b5d0c9"), null, new DateTime(2024, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 7 },
                    { new Guid("c2b5a4f3-7e0d-42c1-e6d8-f9a2b1c0e5d4"), null, new DateTime(2024, 5, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 7 },
                    { new Guid("c8b1a0f9-3e6d-48c7-e2d4-f5a8b7c6e1d0"), null, new DateTime(2024, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 9 },
                    { new Guid("d3c6b5a4-8f1e-43d2-f7e9-a0b3c2d1f6e5"), null, new DateTime(2024, 5, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 8 },
                    { new Guid("d9c2b1a0-4f7e-49d8-f3e5-a6b9c8d7f2e1"), null, new DateTime(2024, 5, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("e0d3c2b1-5a8f-40e9-a4f6-b7c0d9e8a3f2"), null, new DateTime(2024, 5, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 10 },
                    { new Guid("e4d7c6b5-9a2f-44e3-a8f0-b1c4d3e2a7f6"), null, new DateTime(2024, 5, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("f5e8d7c6-0b3a-45f4-b9a1-c2d5e4f3b8a7"), null, new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 10 },
                    { new Guid("f9e2d1c0-4b7a-49f8-b3a5-c6d9e8f7b2a1"), null, new DateTime(2024, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a0f3e2d1-5c8b-40a9-c4b6-d7e0f9a8c3b2"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a6f9e8d7-1c4b-46a5-c0b2-d3e6f5a4c9b8"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b1a4f3e2-6d9c-41b0-d5c7-e8f1a0b9d4c3"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b7a0f9e8-2d5c-47b6-d1c3-e4f7a6b5d0c9"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c2b5a4f3-7e0d-42c1-e6d8-f9a2b1c0e5d4"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c8b1a0f9-3e6d-48c7-e2d4-f5a8b7c6e1d0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d3c6b5a4-8f1e-43d2-f7e9-a0b3c2d1f6e5"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d9c2b1a0-4f7e-49d8-f3e5-a6b9c8d7f2e1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e0d3c2b1-5a8f-40e9-a4f6-b7c0d9e8a3f2"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e4d7c6b5-9a2f-44e3-a8f0-b1c4d3e2a7f6"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f5e8d7c6-0b3a-45f4-b9a1-c2d5e4f3b8a7"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f9e2d1c0-4b7a-49f8-b3a5-c6d9e8f7b2a1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("e8f1a7c9-3d6b-48e2-90f7-b5c2a8d1e647"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("56c24d9d-a605-4ed5-991e-cac7944cd747"));
        }
    }
}
