using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class ForumCategories1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "forums",
                table: "ForumsCategories",
                columns: new[] { "CategoryID", "ForumID" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 3 },
                    { 2, 1 },
                    { 2, 2 },
                    { 2, 3 },
                    { 4, 2 },
                    { 4, 3 },
                    { 5, 2 },
                    { 8, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "forums",
                table: "ForumsCategories",
                keyColumns: new[] { "CategoryID", "ForumID" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                schema: "forums",
                table: "ForumsCategories",
                keyColumns: new[] { "CategoryID", "ForumID" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                schema: "forums",
                table: "ForumsCategories",
                keyColumns: new[] { "CategoryID", "ForumID" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                schema: "forums",
                table: "ForumsCategories",
                keyColumns: new[] { "CategoryID", "ForumID" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                schema: "forums",
                table: "ForumsCategories",
                keyColumns: new[] { "CategoryID", "ForumID" },
                keyValues: new object[] { 2, 3 });

            migrationBuilder.DeleteData(
                schema: "forums",
                table: "ForumsCategories",
                keyColumns: new[] { "CategoryID", "ForumID" },
                keyValues: new object[] { 4, 2 });

            migrationBuilder.DeleteData(
                schema: "forums",
                table: "ForumsCategories",
                keyColumns: new[] { "CategoryID", "ForumID" },
                keyValues: new object[] { 4, 3 });

            migrationBuilder.DeleteData(
                schema: "forums",
                table: "ForumsCategories",
                keyColumns: new[] { "CategoryID", "ForumID" },
                keyValues: new object[] { 5, 2 });

            migrationBuilder.DeleteData(
                schema: "forums",
                table: "ForumsCategories",
                keyColumns: new[] { "CategoryID", "ForumID" },
                keyValues: new object[] { 8, 1 });
        }
    }
}
