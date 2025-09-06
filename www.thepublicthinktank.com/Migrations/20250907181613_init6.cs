using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("af37ce9d-db74-43f1-964f-f6fcb394ec9e"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), null, "Social media platforms should empower users with direct control over the algorithms that determine what content they see, specifically designed to mitigate political polarization and exposure to extremist content. This solution puts decision-making power back in users' hands rather than defaulting to engagement-maximizing algorithms that often amplify divisive content.\n\nThe key feature would be a transparent, user-friendly control panel offering adjustable settings including:\n\n- Political diversity sliders: Users could set preferences for seeing content across the political spectrum rather than only views that align with their existing positions\n\n- Content variety controls: Options to balance news sources, opinion pieces, and user discussions from different perspectives\n\n- Fact-checking intensity: Adjustable settings for how prominently fact-checking information appears alongside political content\n\n- Source credibility thresholds: Ability to set minimum credibility standards for news sources in one's feed\n\n- Tone preferences: Options to prioritize measured, substantive political discussions over inflammatory rhetoric\n\n- Contextual depth settings: Controls for showing more in-depth background on complex political issues rather than simplified, polarizing summaries\n\nThese controls would be accompanied by periodic feedback showing users metrics about their content diet, such as political diversity scores, emotional tone analysis, and source variety statistics. Optional recommendations could suggest small adjustments to experience more balanced political discourse.\n\nImplementation would include educational onboarding to help users understand how their choices affect their information ecosystem, default settings designed for balanced exposure, and continuous refinement based on research about what settings most effectively reduce polarization while maintaining user satisfaction.\n\nBy transferring algorithm control from platform to user, this solution directly addresses the systemic incentives that currently reward divisive content. It preserves free expression while creating pathways for users to intentionally construct healthier information environments that promote understanding across political divides rather than deepening them.", 1, new DateTime(2024, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), new Guid("af37ce9d-db74-43f1-964f-f6fcb394ec9e"), "Make Algorithms User-Adjustable" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a0b1c2d9-8e5f-40a6-c4d1-b7f0e8a5d3c2"), null, new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("a6b7c8d5-4e1f-46a2-c0d7-b3f6e4a1d9c8"), null, new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 7 },
                    { new Guid("b1c2d3e0-9f6a-41b7-d5e2-c8a1f9b6e4d3"), null, new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 7 },
                    { new Guid("b7c8d9e6-5f2a-47b3-d1e8-c4a7f5b2e0d9"), null, new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 8 },
                    { new Guid("c2d3e4f1-0a7b-42c8-e6f3-d9b2a0c7f5e4"), null, new DateTime(2024, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("c8d9e0f7-6a3b-48c4-e2f9-d5b8a6c3f1e0"), null, new DateTime(2024, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("d3e4f5a2-1b8c-43d9-f7a4-e0c3b1d8a6f5"), null, new DateTime(2024, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("d9e0f1a8-7b4c-49d5-f3a0-e6c9b7d4a2f1"), null, new DateTime(2024, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 10 },
                    { new Guid("e4f5a6b3-2c9d-44e0-a8b5-f1d4c2e9b7a6"), null, new DateTime(2024, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("e8f9a0b7-6c3d-48e4-a2b9-f5d8c6e3a1b0"), null, new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("f5a6b7c4-3d0e-45f1-b9c6-a2e5d3f0c8b7"), null, new DateTime(2024, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 9 },
                    { new Guid("f9a0b1c8-7d4e-49f5-b3c0-a6e9d7f4b2c1"), null, new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a0b1c2d9-8e5f-40a6-c4d1-b7f0e8a5d3c2"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a6b7c8d5-4e1f-46a2-c0d7-b3f6e4a1d9c8"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b1c2d3e0-9f6a-41b7-d5e2-c8a1f9b6e4d3"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b7c8d9e6-5f2a-47b3-d1e8-c4a7f5b2e0d9"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c2d3e4f1-0a7b-42c8-e6f3-d9b2a0c7f5e4"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c8d9e0f7-6a3b-48c4-e2f9-d5b8a6c3f1e0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d3e4f5a2-1b8c-43d9-f7a4-e0c3b1d8a6f5"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d9e0f1a8-7b4c-49d5-f3a0-e6c9b7d4a2f1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e4f5a6b3-2c9d-44e0-a8b5-f1d4c2e9b7a6"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e8f9a0b7-6c3d-48e4-a2b9-f5d8c6e3a1b0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f5a6b7c4-3d0e-45f1-b9c6-a2e5d3f0c8b7"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f9a0b1c8-7d4e-49f5-b3c0-a6e9d7f4b2c1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("af37ce9d-db74-43f1-964f-f6fcb394ec9e"));
        }
    }
}
