using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("1c93c95b-7842-440d-aeb9-cdbd61e1fb2f"),
                columns: new[] { "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { "[4,1]", "[2,1]", "[6,5]", "[2]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("2ed7e15a-569a-4d5f-b157-96c8052e3d46"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[4,5]", "[1,0]", "[2]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("b1454543-b9ec-4bb9-9109-edc6a88bb065"),
                columns: new[] { "EntityTypes", "Timeframes" },
                values: new object[] { "[1,3]", "[2]" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("1c93c95b-7842-440d-aeb9-cdbd61e1fb2f"),
                columns: new[] { "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { "[]", "[]", "[6]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("2ed7e15a-569a-4d5f-b157-96c8052e3d46"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("b1454543-b9ec-4bb9-9109-edc6a88bb065"),
                columns: new[] { "EntityTypes", "Timeframes" },
                values: new object[] { "[3,1]", "[3,2]" });
        }
    }
}
