using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Atlas.ConsoleApp.Storage.Migrations;

[DbContext(typeof(AtlasDataContext))]
[Migration("20260929180000_InitialSqlStorage")]
internal sealed class InitialSqlStorage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.CreateTable(
            name: "AtlasCollections",
            columns: table => new
            {
                Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AtlasCollections", x => x.Name));

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "AtlasCollections");
}
