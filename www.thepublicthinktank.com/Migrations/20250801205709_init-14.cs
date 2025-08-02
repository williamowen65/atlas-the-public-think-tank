using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "Title" },
                values: new object[] { new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), null, "How can the think tank be accessible across languages, cultures, and digital literacy levels?\n\nFor a platform like Atlas to achieve its goal of harnessing collective intelligence for problem-solving, it must be accessible to diverse participants worldwide. However, current collaborative platforms often face significant barriers related to language, cultural context, and varying levels of digital literacy.\n\nLanguage barriers can exclude valuable perspectives, while cultural differences in communication styles and norms may lead to misunderstandings or alienation. Additionally, complex interfaces and features can exclude participants with limited digital experience or access to technology.\n\nKey questions include:\n\n- What translation and localization approaches can make content accessible while preserving nuance and context?\n\n- How can user interfaces be designed to be intuitive across cultural contexts and digital literacy levels?\n\n- What alternative access methods could accommodate participants with limited internet connectivity or devices?\n\n- How can the platform's information architecture accommodate different cultural frameworks for organizing knowledge?\n\n- What community norms and facilitation approaches can bridge cultural differences in communication styles?\n\n- How can content moderation be culturally sensitive while maintaining consistent standards?\n\n- What technical solutions might reduce bandwidth requirements for participation?\n\nAddressing these challenges is essential for building a truly global collaborative platform that can leverage diverse perspectives from around the world, rather than only those from privileged communities with high technological access and specific cultural backgrounds.", 1, new DateTime(2024, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Translation and Global Accessibility" });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a0f9e8d7-c6b5-4423-a132-f9e8d7c6b5a0"), null, new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 10 },
                    { new Guid("a6f5e4d3-c2b1-4089-a798-f5e4d3c2b1a6"), null, new DateTime(2024, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee45"), 8 },
                    { new Guid("b1a0f9e8-d7c6-4534-b243-a0f9e8d7c6b1"), null, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("c2b1a0f9-e8d7-4645-c354-b1a0f9e8d7c2"), null, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("d3c2b1a0-f9e8-4756-d465-c2b1a0f9e8d3"), null, new DateTime(2024, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("e4d3c2b1-a0f9-4867-e576-d3c2b1a0f9e4"), null, new DateTime(2024, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 10 },
                    { new Guid("f5e4d3c2-b1a0-4978-f687-e4d3c2b1a0f5"), null, new DateTime(2024, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("f9e8d7c6-b5a4-4312-9021-e8d7c6b5a4f3"), null, new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a0f9e8d7-c6b5-4423-a132-f9e8d7c6b5a0"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a6f5e4d3-c2b1-4089-a798-f5e4d3c2b1a6"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b1a0f9e8-d7c6-4534-b243-a0f9e8d7c6b1"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c2b1a0f9-e8d7-4645-c354-b1a0f9e8d7c2"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d3c2b1a0-f9e8-4756-d465-c2b1a0f9e8d3"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e4d3c2b1-a0f9-4867-e576-d3c2b1a0f9e4"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f5e4d3c2-b1a0-4978-f687-e4d3c2b1a0f5"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f9e8d7c6-b5a4-4312-9021-e8d7c6b5a4f3"));

            migrationBuilder.DeleteData(
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"));
        }
    }
}
