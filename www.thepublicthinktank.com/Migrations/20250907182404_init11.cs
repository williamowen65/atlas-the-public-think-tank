using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("bf2d669f-8a15-4ea9-8a1e-9e3287addb4f"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), null, "Public Education Campaigns to Reduce Stigma represents a strategic approach to changing societal perceptions and attitudes about homelessness through coordinated, evidence-based messaging and community engagement. By tackling misconceptions and humanizing the experience of housing instability, these campaigns can help dismantle one of the most significant barriers preventing people from seeking assistance.\n\nEffective stigma reduction campaigns are multi-faceted, employing various communication channels and approaches. Mass media components utilize billboards, public service announcements, social media campaigns, and traditional advertising to challenge stereotypes and present accurate information about the causes of homelessness, emphasizing structural factors like housing affordability, economic instability, and insufficient support systems rather than personal failings. These campaigns feature authentic stories and images that highlight the diversity of people experiencing homelessness, avoiding sensationalism while preserving dignity.\n\nCommunity engagement initiatives complement mass media efforts through in-person educational workshops, speaking engagements at schools and community organizations, interactive exhibits, and public forums where housed and unhoused community members can engage in facilitated dialogue. These face-to-face interactions help build empathy by creating spaces for genuine connection and understanding.\n\nPeer ambassador programs represent a particularly powerful component, training and employing individuals with lived experience of homelessness to serve as public speakers, media spokespeople, and community educators. This approach not only provides authentic representation but also creates meaningful employment opportunities and recognition of expertise gained through experience.\n\nTargeted professional education reaches service providers, healthcare workers, law enforcement, educators, and other professionals who regularly interact with people experiencing homelessness. This specialized training addresses unconscious bias, promotes trauma-informed approaches, and provides practical strategies for creating more welcoming and dignified service environments.\n\nWhen implemented comprehensively and sustained over time, public education campaigns contribute to measurable shifts in public attitudes, increased support for evidence-based solutions to homelessness, reduced discrimination in service settings, and—most importantly—greater willingness among people experiencing homelessness to seek and engage with available support services.", 1, new DateTime(2024, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), new Guid("bf2d669f-8a15-4ea9-8a1e-9e3287addb4f"), "Public Education Campaigns to Reduce Stigma" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a3d5c4b2-0e9f-43a8-b7b6-2c1d0e9f8a7b"), null, new DateTime(2024, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("a9d1c0b8-6e5f-49a4-b3b2-8c7d6e5f4a3b"), null, new DateTime(2024, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("b0e2d1c9-7f6a-40b5-b4c3-9d8e7f6a5b4c"), null, new DateTime(2024, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("b4e6d5c3-1f0a-44b9-a8c7-3d2e1f0a9b8c"), null, new DateTime(2024, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("b8e0d9c7-5f4a-48b3-a2c1-7d6e5f4a3b2c"), null, new DateTime(2024, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("c1f3e2d0-8a7b-41c6-b5d4-0e9f8a7b6c5d"), null, new DateTime(2024, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("c5f7e6d4-2a1b-45c0-b9d8-4e3f2a1b0c9d"), null, new DateTime(2024, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 9 },
                    { new Guid("c9f1e0d8-6a5b-49c4-b3d2-8e7f6a5b4c3d"), null, new DateTime(2024, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("d0a2f1e9-7b6c-40d5-b4e3-9f8a7b6c5d4e"), null, new DateTime(2024, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("d2a4f3e1-9b8c-42d7-b6e5-1f0a9b8c7d6e"), null, new DateTime(2024, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("d6a8f7e5-3b2c-46d1-b0e9-5f4a3b2c1d0e"), null, new DateTime(2024, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 8 },
                    { new Guid("e1b3a2f0-8c7d-41e6-b5f4-0a9b8c7d6e5f"), null, new DateTime(2024, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("e7b9a8f6-4c3d-47e2-b1f0-6a5b4c3d2e1f"), null, new DateTime(2024, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("f2c4b3a1-9d8e-42f7-b6a5-1b0c9d8e7f6a"), null, new DateTime(2024, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("f8c0b9a7-5d4e-48f3-b2a1-7b6c5d4e3f2a"), null, new DateTime(2024, 10, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a3d5c4b2-0e9f-43a8-b7b6-2c1d0e9f8a7b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a9d1c0b8-6e5f-49a4-b3b2-8c7d6e5f4a3b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b0e2d1c9-7f6a-40b5-b4c3-9d8e7f6a5b4c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b4e6d5c3-1f0a-44b9-a8c7-3d2e1f0a9b8c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b8e0d9c7-5f4a-48b3-a2c1-7d6e5f4a3b2c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c1f3e2d0-8a7b-41c6-b5d4-0e9f8a7b6c5d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c5f7e6d4-2a1b-45c0-b9d8-4e3f2a1b0c9d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c9f1e0d8-6a5b-49c4-b3d2-8e7f6a5b4c3d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d0a2f1e9-7b6c-40d5-b4e3-9f8a7b6c5d4e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d2a4f3e1-9b8c-42d7-b6e5-1f0a9b8c7d6e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d6a8f7e5-3b2c-46d1-b0e9-5f4a3b2c1d0e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e1b3a2f0-8c7d-41e6-b5f4-0a9b8c7d6e5f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e7b9a8f6-4c3d-47e2-b1f0-6a5b4c3d2e1f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f2c4b3a1-9d8e-42f7-b6a5-1b0c9d8e7f6a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f8c0b9a7-5d4e-48f3-b2a1-7b6c5d4e3f2a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("bf2d669f-8a15-4ea9-8a1e-9e3287addb4f"));
        }
    }
}
