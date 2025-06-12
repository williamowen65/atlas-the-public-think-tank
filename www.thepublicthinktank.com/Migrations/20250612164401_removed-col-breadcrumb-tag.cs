using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class removedcolbreadcrumbtag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreadcrumbTag",
                schema: "solutions",
                table: "Solutions");

            migrationBuilder.DropColumn(
                name: "BreadcrumbTag",
                schema: "issues",
                table: "Issues");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BreadcrumbTag",
                schema: "solutions",
                table: "Solutions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "defaulttag");

            migrationBuilder.AddColumn<string>(
                name: "BreadcrumbTag",
                schema: "issues",
                table: "Issues",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "defaulttag");

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("4aebb16c-b474-4c14-9e5e-4548134cadc8"),
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("b3a72e5d-7c18-4e9f-8d24-67a2c6f35b1d"),
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("c246d67c-427c-40f9-8bb2-b0834e473f7b"),
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("e5d8f6a9-3b7c-42e1-9d85-7f63a4b5c28d"),
                columns: new string[0],
                values: new object[0]);

            migrationBuilder.UpdateData(
                schema: "issues",
                table: "Issues",
                keyColumn: "IssueID",
                keyValue: new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"),
                columns: new string[0],
                values: new object[0]);
        }
    }
}
