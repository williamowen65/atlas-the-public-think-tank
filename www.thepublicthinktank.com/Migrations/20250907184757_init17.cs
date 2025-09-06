using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init17 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("af37ce9d-db74-43f1-964f-f6fcb394ec9e"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[4,0]", "[1,0]", "[2]" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("af37ce9d-db74-43f1-964f-f6fcb394ec9e"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });
        }
    }
}
