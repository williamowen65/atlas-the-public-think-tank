using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("26524034-13d3-4e9c-a361-ddf01271a71f"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[2,4]", "[0,1]", "[3]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("3d3a2d73-23e5-4622-9a82-46d0e6d84d26"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[5,4]", "[0,1]", "[3]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("56c24d9d-a605-4ed5-991e-cac7944cd747"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[0,2]", "[1]", "[3]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("681601f2-ed2c-4188-9221-879efa33cc67"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[4]", "[1]", "[2]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("77f65326-607d-4c65-bff1-424bae316eab"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[1,5]", "[1,0]", "[3]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("a059b123-f631-4774-a866-33ba0bb415b5"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[1,5]", "[2,3,1]", "[3,2]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("bf2d669f-8a15-4ea9-8a1e-9e3287addb4f"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[2,5]", "[1,2]", "[2]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("c65ca915-45d4-4975-929b-9a79199dc51f"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[4,2]", "[1]", "[3]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("f89199d9-3218-4712-8362-5c21e4e741c2"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[1,5]", "[2,1]", "[3,2]" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("26524034-13d3-4e9c-a361-ddf01271a71f"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("3d3a2d73-23e5-4622-9a82-46d0e6d84d26"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("56c24d9d-a605-4ed5-991e-cac7944cd747"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("681601f2-ed2c-4188-9221-879efa33cc67"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("77f65326-607d-4c65-bff1-424bae316eab"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("a059b123-f631-4774-a866-33ba0bb415b5"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("bf2d669f-8a15-4ea9-8a1e-9e3287addb4f"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("c65ca915-45d4-4975-929b-9a79199dc51f"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("f89199d9-3218-4712-8362-5c21e4e741c2"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });
        }
    }
}
