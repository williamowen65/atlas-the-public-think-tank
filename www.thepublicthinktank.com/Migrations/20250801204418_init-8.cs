using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"),
                column: "Content",
                value: "How can high-quality ideas from everyday users be surfaced without being buried by noise or popularity bias?\n\nIn collaborative platforms like Atlas, ensuring that high-quality contributions receive appropriate visibility is crucial for maintaining user engagement and facilitating problem-solving.\n\nCurrently, many platforms struggle with this challenge: valuable content can be buried while sensationalist or low-quality content rises to prominence. This undermines the collective intelligence of online communities and discourages thoughtful participation.\n\nKey questions include:\n\n- How can we design discovery mechanisms that surface valuable content without creating perverse incentives?\n\n- What balance should be struck between algorithmic and human curation?\n\n- How can we ensure that new contributors have a fair chance at visibility while still maintaining quality standards?\n\n- What metrics beyond simple engagement best indicate the actual value of contributions?\n\nThese challenges are particularly relevant for a platform like Atlas that aims to harness collective intelligence for problem-solving rather than simply maximizing engagement.");

            migrationBuilder.InsertData(
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), null, "How do you ensure contributors are credited appropriately, especially if their ideas are developed or repurposed by others?\n\nIn collaborative problem-solving environments like Atlas, ideas often evolve through iterative refinement and combination with other perspectives. While this process is essential for developing robust solutions, it presents challenges for ensuring proper attribution and recognition of intellectual contributions.\n\nTraditional intellectual property frameworks are often ill-suited for collaborative platforms where the goal is shared knowledge creation rather than exclusive ownership. Yet, proper attribution remains crucial for maintaining trust, encouraging participation, and respecting contributors' work.\n\nKey questions include:\n\n- What mechanisms can track the provenance of ideas as they evolve through collaborative refinement?\n\n- How can we balance recognition of original contributors with acknowledgment of those who significantly develop or improve ideas?\n\n- What role should automated systems play in tracking contributions versus relying on community norms and practices?\n\n- How can attribution be made transparent without creating excessive overhead that impedes collaboration?\n\n- What recourse should be available when contributors feel their contributions have been misattributed or used without proper credit?\n\nSolving these challenges is essential for creating a collaborative environment where contributors feel their work is respected and valued, while still enabling the free flow of ideas necessary for collective problem-solving.", 1, new DateTime(2024, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Intellectual Property and Attribution" },
                    { new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), null, "How can the Atlas platform prevent or mitigate users who post misleading information, trolls, or coordinated disinformation efforts?\n\nTraditional social media platforms struggle with combating misinformation and bad faith participation without resorting to heavy-handed moderation that risks stifling legitimate discourse. This challenge is particularly acute for a platform like Atlas that aims to foster collaborative problem-solving.\n\nKey questions include:\n\n- What verification mechanisms can be implemented that balance accuracy with accessibility?\n\n- How can the platform's reputation system be designed to reward good-faith participation while discouraging manipulation?\n\n- What role should community moderation play versus automated systems?\n\n- How can the platform distinguish between honest mistakes and deliberate misinformation?\n\n- What safeguards can prevent coordinated manipulation campaigns while protecting privacy?\n\nAddressing these challenges is essential for maintaining the integrity of discussions and ensuring that Atlas fulfills its potential as a space for meaningful collaborative problem-solving rather than becoming another vector for misinformation.", 1, new DateTime(2024, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Misinformation and Bad Faith Participation" }
                });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a3b4c5d6-e7f8-9a0b-1c2d-3e4f5a6b7c8d"), null, new DateTime(2024, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 10 },
                    { new Guid("a4b5c6d7-e8f9-0a1b-2c3d-4e5f6a7b8c9d"), null, new DateTime(2024, 7, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 10 },
                    { new Guid("b4c5d6e7-f8a9-0b1c-2d3e-4f5a6b7c8d9e"), null, new DateTime(2024, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e"), null, new DateTime(2024, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 8 },
                    { new Guid("c5d6e7f8-a9b0-1c2d-3e4f-5a6b7c8d9e0f"), null, new DateTime(2024, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("c6d7e8f9-a0b1-2c3d-4e5f-6a7b8c9d0e1f"), null, new DateTime(2024, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 9 },
                    { new Guid("d1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a"), null, new DateTime(2024, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 8 },
                    { new Guid("e1f2a3b4-c5d6-7e8f-9a0b-1c2d3e4f5a6b"), null, new DateTime(2024, 7, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("e2f3a4b5-c6d7-8e9f-0a1b-2c3d4e5f6a7b"), null, new DateTime(2024, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("f2e3d4c5-b6a7-8f9e-0a1b-2c3d4e5f6a7b"), null, new DateTime(2024, 7, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("f3a4b5c6-d7e8-9f0a-1b2c-3d4e5f6a7b8c"), null, new DateTime(2024, 7, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 7 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a3b4c5d6-e7f8-9a0b-1c2d-3e4f5a6b7c8d"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a4b5c6d7-e8f9-0a1b-2c3d-4e5f6a7b8c9d"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b4c5d6e7-f8a9-0b1c-2d3e-4f5a6b7c8d9e"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c5d6e7f8-a9b0-1c2d-3e4f-5a6b7c8d9e0f"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c6d7e8f9-a0b1-2c3d-4e5f-6a7b8c9d0e1f"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e1f2a3b4-c5d6-7e8f-9a0b-1c2d3e4f5a6b"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e2f3a4b5-c6d7-8e9f-0a1b-2c3d4e5f6a7b"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f2e3d4c5-b6a7-8f9e-0a1b-2c3d4e5f6a7b"));

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f3a4b5c6-d7e8-9f0a-1b2c-3d4e5f6a7b8c"));

            migrationBuilder.DeleteData(
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"));

            migrationBuilder.DeleteData(
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"));

            migrationBuilder.UpdateData(
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"),
                column: "Content",
                value: "In collaborative platforms like Atlas, ensuring that high-quality contributions receive appropriate visibility is crucial for maintaining user engagement and facilitating problem-solving.\n\nCurrently, many platforms struggle with this challenge: valuable content can be buried while sensationalist or low-quality content rises to prominence. This undermines the collective intelligence of online communities and discourages thoughtful participation.\n\nKey questions include:\n\n- How can we design discovery mechanisms that surface valuable content without creating perverse incentives?\n\n- What balance should be struck between algorithmic and human curation?\n\n- How can we ensure that new contributors have a fair chance at visibility while still maintaining quality standards?\n\n- What metrics beyond simple engagement best indicate the actual value of contributions?\n\nThese challenges are particularly relevant for a platform like Atlas that aims to harness collective intelligence for problem-solving rather than simply maximizing engagement.");
        }
    }
}
