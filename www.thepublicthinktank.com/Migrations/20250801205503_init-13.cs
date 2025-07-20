using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "Title" },
                values: new object[] { new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), null, "Should users be able to post anonymously or pseudonymously? How does that affect accountability and trust?\n\nThe question of identity and attribution on collaborative platforms presents a fundamental tension between competing values. On one hand, anonymity and pseudonymity can enable participation from vulnerable populations, protect against retaliation, and allow ideas to be evaluated on their merits rather than their source. On the other hand, these practices can reduce accountability, enable harassment, and potentially undermine trust in the system.\n\nFor a platform like Atlas that aims to foster collective problem-solving, navigating this tension is particularly important. The credibility of solutions may depend on transparent expertise, while the diversity of perspectives may require protecting contributors' identities in some contexts.\n\nKey questions include:\n\n- What granular options between full identification and complete anonymity might provide appropriate balance for different contexts?\n\n- How can reputation systems function effectively when identities may be fluid or concealed?\n\n- What verification mechanisms might establish credibility without requiring full identity disclosure?\n\n- How can platforms prevent abuse of anonymity while preserving its benefits for legitimate uses?\n\n- What community norms and technical systems can establish trust in contributions despite potential identity concealment?\n\n- How might different types of content or actions require different levels of identity verification?\n\nBalancing these considerations requires thoughtful design that respects both the values of transparency and the legitimate needs for privacy and protection in online discourse.", 1, new DateTime(2024, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Balancing Transparency with Anonymity" });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a4f3e2d1-c0b9-4685-c4d3-f1a0e9b8c7d6"), null, new DateTime(2024, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 10 },
                    { new Guid("b5a4f3e2-d1c0-4796-d5e4-a2b1f0c9d8e7"), null, new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("c6b5a4f3-e2d1-4807-e6f5-b3c2a1d0e9f8"), null, new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 6 },
                    { new Guid("d1c0b9a8-7f6e-4352-91a0-c8d7b6a5f4e3"), null, new DateTime(2024, 8, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 8 },
                    { new Guid("d7c6b5a4-f3e2-4918-f7a6-c4d3b2e1f0a9"), null, new DateTime(2024, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("e2d1c0b9-8a7f-4463-a2b1-d9e8c7f6a5b4"), null, new DateTime(2024, 8, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("e8d7c6b5-a4f3-4029-a8b7-d5e4c3f2a1b0"), null, new DateTime(2024, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 7 },
                    { new Guid("f3e2d1c0-9b8a-4574-b3c2-e0f9d8a7b6c5"), null, new DateTime(2024, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 7 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a4f3e2d1-c0b9-4685-c4d3-f1a0e9b8c7d6"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b5a4f3e2-d1c0-4796-d5e4-a2b1f0c9d8e7"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c6b5a4f3-e2d1-4807-e6f5-b3c2a1d0e9f8"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d1c0b9a8-7f6e-4352-91a0-c8d7b6a5f4e3"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d7c6b5a4-f3e2-4918-f7a6-c4d3b2e1f0a9"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e2d1c0b9-8a7f-4463-a2b1-d9e8c7f6a5b4"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e8d7c6b5-a4f3-4029-a8b7-d5e4c3f2a1b0"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f3e2d1c0-9b8a-4574-b3c2-e0f9d8a7b6c5"));

            migrationBuilder.DeleteData(
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"));
        }
    }
}
