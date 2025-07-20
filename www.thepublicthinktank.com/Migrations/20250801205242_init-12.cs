using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "Title" },
                values: new object[] { new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), null, "What strategies can maintain user interest and participation beyond the initial launch or viral phase?\n\nWhile many platforms experience strong initial engagement, sustaining meaningful participation over time remains a significant challenge. For Atlas to effectively leverage collective intelligence for problem-solving, it must overcome the tendency toward declining engagement that affects most collaborative platforms.\n\nTraditional social media often relies on addictive design patterns to maintain engagement, but these approaches frequently lead to shallow interaction rather than thoughtful participation. A platform focused on collaborative problem-solving requires different strategies to sustain long-term community involvement.\n\nKey questions include:\n\n- How can the platform create meaningful progression systems that reward deepening contribution without gamifying in ways that distort participation?\n\n- What feedback mechanisms best help users understand their impact and the value of their contributions?\n\n- How can community rituals and regular events create sustainable rhythms of participation?\n\n- What role should real-world impact and implementation of solutions play in maintaining motivation?\n\n- How can the platform support different modes of engagement that accommodate varying levels of time commitment and expertise?\n\n- What governance structures allow the community to evolve with changing needs while maintaining coherent purpose?\n\n- How can the platform encourage meaningful relationships between participants that strengthen commitment to the community?\n\nAddressing these challenges requires balancing intrinsic and extrinsic motivations while creating structures that support sustained, meaningful participation without burnout or disillusionment.", 1, new DateTime(2024, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Sustaining Long-Term Engagement" });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("2d3e4f5a-6b7c-4809-a1b2-c3d4e5f6a7b8"), null, new DateTime(2024, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 7 },
                    { new Guid("3c4d5e6f-7a8b-4921-83c0-f5e4d3c2b1a0"), null, new DateTime(2024, 8, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("9a8b7c6d-5f4e-4312-b0a9-1c2d3e4f5a6b"), null, new DateTime(2024, 8, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 8 },
                    { new Guid("a9b8c7d6-e5f4-4312-b1a0-9d8c7b6a5f4e"), null, new DateTime(2024, 8, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 8 },
                    { new Guid("c7b8a9d0-5e2f-4983-a1b0-c9d8e7f6a5b4"), null, new DateTime(2024, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 7 },
                    { new Guid("e5f4d3c2-b1a0-4675-8392-c1d0b9a8f7e6"), null, new DateTime(2024, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 10 },
                    { new Guid("f1e2d3c4-b5a6-4798-87b9-d0c1e2f3a4b5"), null, new DateTime(2024, 8, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("2d3e4f5a-6b7c-4809-a1b2-c3d4e5f6a7b8"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("3c4d5e6f-7a8b-4921-83c0-f5e4d3c2b1a0"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("9a8b7c6d-5f4e-4312-b0a9-1c2d3e4f5a6b"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a9b8c7d6-e5f4-4312-b1a0-9d8c7b6a5f4e"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c7b8a9d0-5e2f-4983-a1b0-c9d8e7f6a5b4"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e5f4d3c2-b1a0-4675-8392-c1d0b9a8f7e6"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f1e2d3c4-b5a6-4798-87b9-d0c1e2f3a4b5"));

            migrationBuilder.DeleteData(
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"));
        }
    }
}
