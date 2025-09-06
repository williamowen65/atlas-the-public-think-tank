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
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("10873349-5aaa-4e6f-a08a-cbbe1ac2127f"),
                columns: new[] { "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { "[1,5]", "[2,1]", "[3,5]", "[2]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("3119d678-b8b9-4c6f-90f7-e35e5d446dfc"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[4,0]", "[1,2]", "[2]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("76bfbcac-97fb-4ee0-b128-d0089e53149c"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[4,2]", "[0,1]", "[3]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("81b1b7fc-c732-4c6b-a888-db7ecb9d5609"),
                columns: new[] { "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { "[4,5]", "[0]", "[1,6]", "[3]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("adb98c92-49b8-47bb-b652-e4917e055150"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[2,4]", "[0,1]", "[3]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("b1454543-b9ec-4bb9-9109-edc6a88bb065"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[4,1]", "[3,1]", "[3,2]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("c28f380c-fa61-4113-a38f-d16a4ff4f5f1"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[0,2]", "[2,1]", "[3,2]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("e9f82158-76e9-4071-87d5-4e0d8d9d99a6"),
                columns: new[] { "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { "[2,5]", "[0,1]", "[3,6]", "[2]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("fa86f462-e7ed-47b4-8c86-c6cc5bfc02e8"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[0,2]", "[0,1]", "[3,2]" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("10873349-5aaa-4e6f-a08a-cbbe1ac2127f"),
                columns: new[] { "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { "[]", "[]", "[6]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("3119d678-b8b9-4c6f-90f7-e35e5d446dfc"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("76bfbcac-97fb-4ee0-b128-d0089e53149c"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("81b1b7fc-c732-4c6b-a888-db7ecb9d5609"),
                columns: new[] { "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { "[]", "[]", "[6]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("adb98c92-49b8-47bb-b652-e4917e055150"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("b1454543-b9ec-4bb9-9109-edc6a88bb065"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("c28f380c-fa61-4113-a38f-d16a4ff4f5f1"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("e9f82158-76e9-4071-87d5-4e0d8d9d99a6"),
                columns: new[] { "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { "[]", "[]", "[6]", "[]" });

            migrationBuilder.UpdateData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("fa86f462-e7ed-47b4-8c86-c6cc5bfc02e8"),
                columns: new[] { "Domains", "EntityTypes", "Timeframes" },
                values: new object[] { "[]", "[]", "[]" });
        }
    }
}
