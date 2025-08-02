using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "Title" },
                values: new object[] { new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), null, "What kind of moderation is required to keep discourse civil, inclusive, and focused—without being overly censorious?\n\nCreating an environment for productive problem-solving requires balancing freedom of expression with the need for respectful, constructive dialogue. Traditional moderation approaches often struggle with this balance, either allowing harmful behavior that drives away valuable contributors or implementing restrictions that stifle legitimate discussion.\n\nFor a platform like Atlas that aims to harness collective intelligence, this challenge is particularly critical. The governance model must support robust debate while preventing the toxicity that plagues many online spaces.\n\nKey questions include:\n\n- How can moderation systems distinguish between passionate disagreement and harmful behavior?\n\n- What role should community governance play versus centralized moderation?\n\n- How can moderation decisions be made transparent and accountable?\n\n- What escalation paths should exist when users disagree with moderation decisions?\n\n- How can the platform's design itself encourage constructive behavior and reduce the need for active moderation?\n\n- What metrics can measure the health of discourse without creating perverse incentives?\n\nDeveloping effective governance models is essential for creating an environment where diverse perspectives can contribute to solving complex problems without descending into unproductive conflict.", 1, new DateTime(2024, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Moderation and Governance of Public Debates" });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a5b6c7d8-e9f0-1a2b-3c4d-5e6f7a8b9c0d"), null, new DateTime(2024, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 9 },
                    { new Guid("b6c7d8e9-f0a1-2b3c-4d5e-6f7a8b9c0d1e"), null, new DateTime(2024, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 8 },
                    { new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"), null, new DateTime(2024, 8, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("c7d8e9f0-a1b2-3c4d-5e6f-7a8b9c0d1e2f"), null, new DateTime(2024, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("d2e3f4a5-b6c7-8d9e-0f1a-2b3c4d5e6f7a"), null, new DateTime(2024, 8, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("e3f4a5b6-c7d8-9e0f-1a2b-3c4d5e6f7a8b"), null, new DateTime(2024, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 10 },
                    { new Guid("f4a5b6c7-d8e9-0f1a-2b3c-4d5e6f7a8b9c"), null, new DateTime(2024, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 7 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a5b6c7d8-e9f0-1a2b-3c4d-5e6f7a8b9c0d"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b6c7d8e9-f0a1-2b3c-4d5e-6f7a8b9c0d1e"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c7d8e9f0-a1b2-3c4d-5e6f-7a8b9c0d1e2f"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d2e3f4a5-b6c7-8d9e-0f1a-2b3c4d5e6f7a"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e3f4a5b6-c7d8-9e0f-1a2b-3c4d5e6f7a8b"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f4a5b6c7-d8e9-0f1a-2b3c-4d5e6f7a8b9c"));

            migrationBuilder.DeleteData(
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"));
        }
    }
}
