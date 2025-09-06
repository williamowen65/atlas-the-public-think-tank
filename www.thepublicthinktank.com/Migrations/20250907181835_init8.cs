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
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("c65ca915-45d4-4975-929b-9a79199dc51f"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Posts are made of blocks — Problem, Evidence, Opinion, Ask — to improve clarity.\n\nSocial media platforms should implement 'Modular Posting' as a structured content creation framework that breaks posts into distinct, labeled components. This solution would transform the standard free-form posting format into a more organized approach that helps both creators and readers distinguish between different types of information.\n\nKey modules would include:\n\n- Problem: A clearly defined issue or question being addressed\n\n- Evidence: Factual information, data, or sources supporting claims\n\n- Opinion: Clearly marked personal perspectives or interpretations\n\n- Ask: Specific calls to action, questions for discussion, or requests\n\nAdditional features would include:\n\n- Visual differentiation: Each module would have distinct styling to make the post structure immediately apparent to readers\n\n- Flexible ordering: Users could arrange modules in the sequence that best serves their communication goals\n\n- Optional modules: Not all posts would require all module types, allowing flexibility while maintaining structure\n\n- Advanced filtering: Readers could filter content based on module types (e.g., 'show me posts with evidence')\n\nThis approach would significantly improve content clarity by helping users distinguish between facts, opinions, and requests. It would encourage more thoughtful content creation by prompting users to consider different aspects of their communication. For readers, modular posts would enable faster comprehension and more effective evaluation of information. This structure would be particularly valuable for complex topics where mixing different types of information often leads to misunderstandings and unnecessary conflicts.", 1, new DateTime(2024, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("c65ca915-45d4-4975-929b-9a79199dc51f"), "Modular Posting" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("96b5c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7c"), null, new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 7 },
                    { new Guid("a6b5c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7b"), null, new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("b5a6c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7a"), null, new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 8 },
                    { new Guid("c4f1e2d3-a5b6-47a8-9b0c-1d2e3f4a5b6f"), null, new DateTime(2024, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 6 },
                    { new Guid("d3c4f1e2-b6a5-47a8-9b0c-1d2e3f4a5b6e"), null, new DateTime(2024, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("e2d3c4f1-a5b6-47a8-9b0c-1d2e3f4a5b6d"), null, new DateTime(2024, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 7 },
                    { new Guid("f1e2d3c4-b5a6-47a8-9b0c-1d2e3f4a5b6c"), null, new DateTime(2024, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("96b5c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a6b5c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b5a6c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c4f1e2d3-a5b6-47a8-9b0c-1d2e3f4a5b6f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d3c4f1e2-b6a5-47a8-9b0c-1d2e3f4a5b6e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e2d3c4f1-a5b6-47a8-9b0c-1d2e3f4a5b6d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f1e2d3c4-b5a6-47a8-9b0c-1d2e3f4a5b6c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("c65ca915-45d4-4975-929b-9a79199dc51f"));
        }
    }
}
