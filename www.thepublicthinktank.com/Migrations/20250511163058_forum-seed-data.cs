using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class forumseeddata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "forums",
                table: "Forums",
                columns: new[] { "ForumID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentForumID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { 1, "1a61454c-5b83-4aab-8661-96d6dffbee30", null, "A forum to discuss practical solutions to climate change at individual and policy levels.", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 1, "Climate Change Solutions" },
                    { 2, "1a61454c-5b83-4aab-8661-96d6dffbee30", null, "Discussion on modern urban planning approaches for sustainable and livable cities.", 1, new DateTime(2024, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 2, "Urban Planning Innovations" },
                    { 3, "1a61454c-5b83-4aab-8661-96d6dffbe31", null, "Strategies for transitioning to renewable energy sources at community and national levels.", 1, new DateTime(2024, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, "Renewable Energy Transition" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "forums",
                table: "Forums",
                keyColumn: "ForumID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "forums",
                table: "Forums",
                keyColumn: "ForumID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "forums",
                table: "Forums",
                keyColumn: "ForumID",
                keyValue: 1);
        }
    }
}
