using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issues",
                table: "IssueVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("6a7b8c9d-0e1f-42a3-b4c5-6d7e8f9a0b1c"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[] { new Guid("6a7b8c9d-0e1f-42a3-b4c5-6d7e8f9a0b1c"), null, new DateTime(2024, 4, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 });
        }
    }
}
