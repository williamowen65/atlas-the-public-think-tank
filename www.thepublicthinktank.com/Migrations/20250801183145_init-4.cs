using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e3f9d8c7-0b54-49a2-b187-4d60e5f3a9b8"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[] { new Guid("e3f9d8c7-0b54-49a2-b187-4d60e5f3a9b8"), null, new DateTime(2024, 10, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 });
        }
    }
}
