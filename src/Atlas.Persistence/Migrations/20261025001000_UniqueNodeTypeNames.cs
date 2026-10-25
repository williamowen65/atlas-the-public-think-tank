using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations;

/// <summary>Make readable type-name lookup unambiguous, including concurrent creation.</summary>
public partial class UniqueNodeTypeNames : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Stop with an actionable error instead of deleting or merging existing graph references.
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM [NodeTypeRows]
                GROUP BY [Name] COLLATE Latin1_General_100_CI_AS_SC HAVING COUNT(*) > 1)
                THROW 51000, 'Duplicate node type names must be resolved before applying UniqueNodeTypeNames.', 1;
            """);
        migrationBuilder.AlterColumn<string>(
            name: "Name", table: "NodeTypeRows", type: "nvarchar(450)", maxLength: 450,
            nullable: false, collation: "Latin1_General_100_CI_AS_SC",
            oldClrType: typeof(string), oldType: "nvarchar(450)", oldMaxLength: 450);
        migrationBuilder.CreateIndex(name: "IX_NodeTypeRows_Name", table: "NodeTypeRows", column: "Name", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_NodeTypeRows_Name", table: "NodeTypeRows");
        migrationBuilder.AlterColumn<string>(
            name: "Name", table: "NodeTypeRows", type: "nvarchar(450)", maxLength: 450,
            nullable: false, collation: "DATABASE_DEFAULT",
            oldClrType: typeof(string), oldType: "nvarchar(450)", oldMaxLength: 450,
            oldCollation: "Latin1_General_100_CI_AS_SC");
    }
}
