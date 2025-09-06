using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("81b1b7fc-c732-4c6b-a888-db7ecb9d5609"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Quietly shows users how long they've been passive vs. active contributors.\n\nSocial media platforms should implement a 'Consumption Tracker' that provides users with insights into their engagement patterns. This solution would offer a subtle but impactful way to increase awareness of passive consumption versus active participation on the platform.\n\nKey features would include:\n\n- Time visualization: An unobtrusive dashboard showing the balance between time spent scrolling/viewing versus time spent creating content, commenting, or otherwise contributing\n\n- Weekly insights: Gentle notifications or summaries that highlight engagement patterns without judgment, celebrating contribution milestones\n\n- Contribution diversity metrics: Tracking different types of engagement (e.g., original posts, thoughtful comments, sharing educational content) to recognize various forms of valuable contribution\n\n- Customizable goals: Optional, user-defined targets for desired participation levels that align with personal digital wellbeing objectives\n\n- Privacy controls: Full user control over tracking data, with options to pause tracking or delete history\n\nUnlike addictive engagement metrics, this solution emphasizes quality of engagement rather than quantity. By making users aware of their consumption-to-contribution ratio, platforms could encourage more mindful usage patterns and foster a community of active participants rather than passive consumers. This approach also empowers users to make informed decisions about their digital habits without employing manipulative design techniques.", 1, new DateTime(2024, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("81b1b7fc-c732-4c6b-a888-db7ecb9d5609"), "Consumption Tracker" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("0e7f8a9b-6c5d-4e3f-1a2b-9c8d7e6f5a4b"), null, new DateTime(2024, 3, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 4 },
                    { new Guid("1f8a9b0c-7d6e-5f4a-2b3c-0d9e8f7a6b5c"), null, new DateTime(2024, 3, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 1 },
                    { new Guid("2a9b0c1d-8e7f-6a5b-3c4d-1e0f9a8b7c6d"), null, new DateTime(2024, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("3b0c1d2e-9f8a-7b6c-4d5e-2f1a0b9c8d7e"), null, new DateTime(2024, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 2 },
                    { new Guid("4c1d2e3f-0a9b-8c7d-5e6f-3a2b1c0d9e8f"), null, new DateTime(2024, 3, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 3 },
                    { new Guid("5d7c8f3a-2e6b-4c1d-9a3f-0e8d7b5c6a4f"), null, new DateTime(2024, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 2 },
                    { new Guid("6a3b5c8d-1e7f-4a9b-8c5d-2e4f6a7b8c9d"), null, new DateTime(2024, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 1 },
                    { new Guid("7b4c6d5e-3f2a-1b9c-8d7e-6f5a4b3c2d1e"), null, new DateTime(2024, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 3 },
                    { new Guid("8c5d6e7f-4a3b-2c1d-9e8f-7a6b5c4d3e2f"), null, new DateTime(2024, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 5 },
                    { new Guid("9d6e7f8a-5b4c-3d2e-0f1a-8b7c6d5e4f3a"), null, new DateTime(2024, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("0e7f8a9b-6c5d-4e3f-1a2b-9c8d7e6f5a4b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("1f8a9b0c-7d6e-5f4a-2b3c-0d9e8f7a6b5c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("2a9b0c1d-8e7f-6a5b-3c4d-1e0f9a8b7c6d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("3b0c1d2e-9f8a-7b6c-4d5e-2f1a0b9c8d7e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("4c1d2e3f-0a9b-8c7d-5e6f-3a2b1c0d9e8f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("5d7c8f3a-2e6b-4c1d-9a3f-0e8d7b5c6a4f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("6a3b5c8d-1e7f-4a9b-8c5d-2e4f6a7b8c9d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("7b4c6d5e-3f2a-1b9c-8d7e-6f5a4b3c2d1e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("8c5d6e7f-4a3b-2c1d-9e8f-7a6b5c4d3e2f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("9d6e7f8a-5b4c-3d2e-0f1a-8b7c6d5e4f3a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("81b1b7fc-c732-4c6b-a888-db7ecb9d5609"));
        }
    }
}
