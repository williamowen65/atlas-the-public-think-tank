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
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("26524034-13d3-4e9c-a361-ddf01271a71f"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "People post as personas to explore ideas, not identities.\n\nSocial media platforms should implement 'Roleplay Threads' as dedicated spaces where users can temporarily adopt different perspectives through clearly marked personas. This solution would create safe environments for exploring diverse viewpoints without the social consequences of permanently associating those views with one's personal identity.\n\nKey features would include:\n\n- Transparent persona creation: Users could create temporary personas with clear labels indicating they're roleplay identities, not authentic personal accounts\n\n- Perspective-based discussions: Threads would be organized around specific topics where users explicitly adopt different philosophical, professional, or cultural perspectives\n\n- Guided facilitation tools: Prompts and frameworks to help users explore ideas fairly and thoroughly from multiple angles\n\n- Civil discourse enforcement: Special moderation rules designed specifically for roleplay spaces to maintain respectful exploration while allowing challenging conversations\n\n- Knowledge-building focus: Emphasis on collaborative learning rather than performance or personal branding\n\nThis approach would transform social media from a place of rigid identity performance to a laboratory for intellectual exploration and empathy development. By separating ideas from identities, platforms could foster more nuanced conversations about complex topics without the polarization that occurs when views become tied to personal brands. Roleplay Threads would provide a structured environment for users to practice perspective-taking and develop a deeper understanding of different worldviews.", 1, new DateTime(2024, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("26524034-13d3-4e9c-a361-ddf01271a71f"), "Roleplay Threads" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a7b-8c9d-1e2f3a4b5c6d"), null, new DateTime(2024, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 7 },
                    { new Guid("a7b8c9d0-e1f2-4a3b-5c6d-7e8f9a0b1c2d"), null, new DateTime(2024, 4, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 3 },
                    { new Guid("b2c3d4e5-f6a7-4b8c-9d0e-2f3a4b5c6d7e"), null, new DateTime(2024, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 5 },
                    { new Guid("b8c9d0e1-f2a3-4b5c-6d7e-8f9a0b1c2d3e"), null, new DateTime(2024, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("c3d4e5f6-a7b8-4c9d-0e1f-3a4b5c6d7e8f"), null, new DateTime(2024, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("d4e5f6a7-b8c9-4d0e-1f2a-4b5c6d7e8f9a"), null, new DateTime(2024, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 4 },
                    { new Guid("e5f6a7b8-c9d0-4e1f-2a3b-5c6d7e8f9a0b"), null, new DateTime(2024, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("f6a7b8c9-d0e1-4f2a-3b4c-6d7e8f9a0b1c"), null, new DateTime(2024, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 6 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a1b2c3d4-e5f6-4a7b-8c9d-1e2f3a4b5c6d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a7b8c9d0-e1f2-4a3b-5c6d-7e8f9a0b1c2d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b2c3d4e5-f6a7-4b8c-9d0e-2f3a4b5c6d7e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b8c9d0e1-f2a3-4b5c-6d7e-8f9a0b1c2d3e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c3d4e5f6-a7b8-4c9d-0e1f-3a4b5c6d7e8f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d4e5f6a7-b8c9-4d0e-1f2a-4b5c6d7e8f9a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e5f6a7b8-c9d0-4e1f-2a3b-5c6d7e8f9a0b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f6a7b8c9-d0e1-4f2a-3b4c-6d7e8f9a0b1c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("26524034-13d3-4e9c-a361-ddf01271a71f"));
        }
    }
}
