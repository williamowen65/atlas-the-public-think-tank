using Microsoft.EntityFrameworkCore.Migrations;

namespace Atlas.Persistence.Migrations;

public partial class ConfigureDiscoveryTextCollation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Title", table: "NodeRows", type: "nvarchar(450)", maxLength: 450,
            nullable: false, collation: DiscoveryTextCollation.Name,
            oldClrType: typeof(string), oldType: "nvarchar(450)", oldMaxLength: 450);
        migrationBuilder.AlterColumn<string>(
            name: "Markdown", table: "BlockRows", type: "nvarchar(max)", nullable: true,
            collation: DiscoveryTextCollation.Name, oldClrType: typeof(string),
            oldType: "nvarchar(max)", oldNullable: true);
        migrationBuilder.AlterColumn<string>(
            name: "AltText", table: "BlockRows", type: "nvarchar(max)", nullable: true,
            collation: DiscoveryTextCollation.Name, oldClrType: typeof(string),
            oldType: "nvarchar(max)", oldNullable: true);
        migrationBuilder.AlterColumn<string>(
            name: "Caption", table: "BlockRows", type: "nvarchar(max)", nullable: true,
            collation: DiscoveryTextCollation.Name, oldClrType: typeof(string),
            oldType: "nvarchar(max)", oldNullable: true);
        migrationBuilder.AlterColumn<string>(
            name: "Title", table: "BlockRows", type: "nvarchar(max)", nullable: true,
            collation: DiscoveryTextCollation.Name, oldClrType: typeof(string),
            oldType: "nvarchar(max)", oldNullable: true);
        migrationBuilder.AlterColumn<string>(
            name: "Description", table: "BlockRows", type: "nvarchar(max)", nullable: true,
            collation: DiscoveryTextCollation.Name, oldClrType: typeof(string),
            oldType: "nvarchar(max)", oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Title", table: "NodeRows", type: "nvarchar(450)", maxLength: 450,
            nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)",
            oldMaxLength: 450, oldCollation: DiscoveryTextCollation.Name);
        migrationBuilder.AlterColumn<string>(
            name: "Markdown", table: "BlockRows", type: "nvarchar(max)", nullable: true,
            oldClrType: typeof(string), oldType: "nvarchar(max)", oldNullable: true,
            oldCollation: DiscoveryTextCollation.Name);
        migrationBuilder.AlterColumn<string>(
            name: "AltText", table: "BlockRows", type: "nvarchar(max)", nullable: true,
            oldClrType: typeof(string), oldType: "nvarchar(max)", oldNullable: true,
            oldCollation: DiscoveryTextCollation.Name);
        migrationBuilder.AlterColumn<string>(
            name: "Caption", table: "BlockRows", type: "nvarchar(max)", nullable: true,
            oldClrType: typeof(string), oldType: "nvarchar(max)", oldNullable: true,
            oldCollation: DiscoveryTextCollation.Name);
        migrationBuilder.AlterColumn<string>(
            name: "Title", table: "BlockRows", type: "nvarchar(max)", nullable: true,
            oldClrType: typeof(string), oldType: "nvarchar(max)", oldNullable: true,
            oldCollation: DiscoveryTextCollation.Name);
        migrationBuilder.AlterColumn<string>(
            name: "Description", table: "BlockRows", type: "nvarchar(max)", nullable: true,
            oldClrType: typeof(string), oldType: "nvarchar(max)", oldNullable: true,
            oldCollation: DiscoveryTextCollation.Name);
    }
}
