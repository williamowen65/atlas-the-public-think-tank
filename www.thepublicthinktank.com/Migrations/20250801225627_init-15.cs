using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"),
                column: "VoteValue",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"),
                column: "VoteValue",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"),
                column: "VoteValue",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"),
                column: "VoteValue",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"),
                column: "VoteValue",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"),
                column: "VoteValue",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"),
                column: "VoteValue",
                value: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"),
                column: "VoteValue",
                value: 7);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"),
                column: "VoteValue",
                value: 9);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"),
                column: "VoteValue",
                value: 6);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"),
                column: "VoteValue",
                value: 8);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"),
                column: "VoteValue",
                value: 5);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"),
                column: "VoteValue",
                value: 3);

            migrationBuilder.UpdateData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"),
                column: "VoteValue",
                value: 7);
        }
    }
}
