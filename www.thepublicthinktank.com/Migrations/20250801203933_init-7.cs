using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "Title" },
                values: new object[] { new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), null, "In collaborative platforms like Atlas, ensuring that high-quality contributions receive appropriate visibility is crucial for maintaining user engagement and facilitating problem-solving.\n\nCurrently, many platforms struggle with this challenge: valuable content can be buried while sensationalist or low-quality content rises to prominence. This undermines the collective intelligence of online communities and discourages thoughtful participation.\n\nKey questions include:\n\n- How can we design discovery mechanisms that surface valuable content without creating perverse incentives?\n\n- What balance should be struck between algorithmic and human curation?\n\n- How can we ensure that new contributors have a fair chance at visibility while still maintaining quality standards?\n\n- What metrics beyond simple engagement best indicate the actual value of contributions?\n\nThese challenges are particularly relevant for a platform like Atlas that aims to harness collective intelligence for problem-solving rather than simply maximizing engagement.", 1, new DateTime(2024, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Discoverability and Visibility of Contributions" });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a4b7c9e1-2d3f-4a5b-6c7d-8e9f0a1b2c3d"), null, new DateTime(2024, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("b5c8d0e2-3f4a-5b6c-7d8e-9f0a1b2c3d4e"), null, new DateTime(2024, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 8 },
                    { new Guid("c6d9e0f3-4a5b-6c7d-8e9f-0a1b2c3d4e5f"), null, new DateTime(2024, 6, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 7 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a4b7c9e1-2d3f-4a5b-6c7d-8e9f0a1b2c3d"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b5c8d0e2-3f4a-5b6c-7d8e-9f0a1b2c3d4e"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c6d9e0f3-4a5b-6c7d-8e9f-0a1b2c3d4e5f"));

            migrationBuilder.DeleteData(
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"));
        }
    }
}
