using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameModeratorToGlobalModerator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("94000000-0000-4000-8000-000000000002"),
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "GlobalModerator", "GLOBALMODERATOR" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("94000000-0000-4000-8000-000000000002"),
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Moderator", "MODERATOR" });
        }
    }
}
