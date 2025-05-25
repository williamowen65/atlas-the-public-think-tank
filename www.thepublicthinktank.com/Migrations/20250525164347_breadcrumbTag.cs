using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class breadcrumbTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

           
            migrationBuilder.AddCheckConstraint(
                name: "CK_Solution_BreadcrumbTag_Length",
                schema: "solutions",
                table: "Solutions",
                sql: "LEN([BreadcrumbTag]) >= 3");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Issue_BreadcrumbTag_Length",
                schema: "issues",
                table: "Issues",
                sql: "LEN([BreadcrumbTag]) >= 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Solution_BreadcrumbTag_Length",
                schema: "solutions",
                table: "Solutions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Issue_BreadcrumbTag_Length",
                schema: "issues",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "BreadcrumbTag",
                schema: "solutions",
                table: "Solutions");

            migrationBuilder.DropColumn(
                name: "BreadcrumbTag",
                schema: "issues",
                table: "Issues");
        }
    }
}
