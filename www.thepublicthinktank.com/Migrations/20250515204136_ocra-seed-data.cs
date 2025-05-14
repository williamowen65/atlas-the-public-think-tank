using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class ocraseeddata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "issues",
                table: "Solutions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.InsertData(
                schema: "issues",
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { 4, "1a61454c-5b83-4aab-8661-96d6dffbe31", null, "The world is experiencing a biodiversity crisis, with thousands of species teetering on the edge of extinction.", 1, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 1, "Critical Decline of Endangered Species" },
                    { 5, "1a61454c-5b83-4aab-8661-96d6dffbe31", null, "This decline is attributed to a combination of factors, including reduced prey availability, pollution, and vessel traffic.", 1, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 2, "The Southern Resident orca population has dropped from 88 individuals in 2010 to just 74 as of late 2024" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "issues",
                table: "Solutions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
