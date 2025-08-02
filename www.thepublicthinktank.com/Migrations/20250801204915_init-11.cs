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
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "Title" },
                values: new object[] { new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), null, "How can the platform ensure diverse voices are heard and prevent dominance by already-privileged demographics?\n\nCollaborative platforms often inadvertently reproduce or amplify existing societal inequalities in who participates and whose contributions receive attention. For a platform like Atlas that aims to leverage collective intelligence to solve complex problems, ensuring diverse participation is not just a matter of fairness but also essential for developing comprehensive, effective solutions.\n\nMany current platforms struggle with representation issues across dimensions like gender, race, socioeconomic status, disability, geographic location, and educational background. These disparities limit the range of perspectives and expertise available to address challenges.\n\nKey questions include:\n\n- What design features can reduce barriers to participation for underrepresented groups?\n\n- How can discovery algorithms be designed to surface valuable contributions from diverse participants rather than reinforcing existing visibility advantages?\n\n- What metrics should be tracked to identify representation gaps without creating privacy concerns?\n\n- How can the platform encourage inclusive dialogue without tokenizing contributors from underrepresented groups?\n\n- What community norms and moderation approaches can prevent behaviors that disproportionately drive away participants from marginalized groups?\n\n- How can the platform's structure acknowledge and address the different resources (time, technical access, etc.) available to different potential participants?\n\nAddressing these challenges requires thoughtful design at all levels—from technical infrastructure to community governance—to create an environment where diverse perspectives can meaningfully contribute to problem-solving.", 1, new DateTime(2024, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Bias and Representation in Participation" });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a0b1c2d3-e4f5-6a7b-8c9d-0e1f2a3b4c5d"), null, new DateTime(2024, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("b1c2d3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e"), null, new DateTime(2024, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("c2d3e4f5-a6b7-8c9d-0e1f-2a3b4c5d6e7f"), null, new DateTime(2024, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("d3e4f5a6-b7c8-9d0e-1f2a-3b4c5d6e7f8a"), null, new DateTime(2024, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("e4f5a6b7-c8d9-0e1f-2a3b-4c5d6e7f8a9b"), null, new DateTime(2024, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("e8f9a0b1-c2d3-4e5f-6a7b-8c9d0e1f2a3b"), null, new DateTime(2024, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 10 },
                    { new Guid("f5a6b7c8-d9e0-1f2a-3b4c-5d6e7f8a9b0c"), null, new DateTime(2024, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("f9a0b1c2-d3e4-5f6a-7b8c-9d0e1f2a3b4c"), null, new DateTime(2024, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 9 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a0b1c2d3-e4f5-6a7b-8c9d-0e1f2a3b4c5d"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b1c2d3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c2d3e4f5-a6b7-8c9d-0e1f-2a3b4c5d6e7f"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d3e4f5a6-b7c8-9d0e-1f2a-3b4c5d6e7f8a"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e4f5a6b7-c8d9-0e1f-2a3b-4c5d6e7f8a9b"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e8f9a0b1-c2d3-4e5f-6a7b-8c9d0e1f2a3b"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f5a6b7c8-d9e0-1f2a-3b4c-5d6e7f8a9b0c"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f9a0b1c2-d3e4-5f6a-7b8c-9d0e1f2a3b4c"));

            migrationBuilder.DeleteData(
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"));
        }
    }
}
