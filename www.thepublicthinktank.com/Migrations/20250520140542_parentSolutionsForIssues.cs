using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class parentSolutionsForIssues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentSolutionID",
                schema: "issues",
                table: "Issues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentSolutionSolutionID",
                schema: "issues",
                table: "Issues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("4aebb16c-b474-4c14-9e5e-4548134cadc8"),
                columns: new[] { "ParentSolutionID", "ParentSolutionSolutionID" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("b3a72e5d-7c18-4e9f-8d24-67a2c6f35b1d"),
                columns: new[] { "ParentSolutionID", "ParentSolutionSolutionID" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("c246d67c-427c-40f9-8bb2-b0834e473f7b"),
                columns: new[] { "ParentSolutionID", "ParentSolutionSolutionID" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("e5d8f6a9-3b7c-42e1-9d85-7f63a4b5c28d"),
                columns: new[] { "ParentSolutionID", "ParentSolutionSolutionID" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"),
                columns: new[] { "ParentSolutionID", "ParentSolutionSolutionID" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ParentSolutionSolutionID",
                schema: "issues",
                table: "Issues",
                column: "ParentSolutionSolutionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Solutions_ParentSolutionSolutionID",
                schema: "issues",
                table: "Issues",
                column: "ParentSolutionSolutionID",
                principalSchema: "solutions",
                principalTable: "Solutions",
                principalColumn: "SolutionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Solutions_ParentSolutionSolutionID",
                schema: "issues",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_ParentSolutionSolutionID",
                schema: "issues",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "ParentSolutionID",
                schema: "issues",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "ParentSolutionSolutionID",
                schema: "issues",
                table: "Issues");
        }
    }
}
