using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNodeShortDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Older Atlas installations indexed ShortDescription along with Title.
            // Database setup recreates the current Title-only index after migration.
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.fulltext_index_columns AS indexed
                    JOIN sys.columns AS col
                        ON col.object_id = indexed.object_id AND col.column_id = indexed.column_id
                    WHERE indexed.object_id = OBJECT_ID(N'dbo.NodeRows')
                        AND col.name = N'ShortDescription'
                )
                    DROP FULLTEXT INDEX ON dbo.NodeRows;
                """, suppressTransaction: true);

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "NodeRows");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "ModerationReportedSnapshotRows");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "NodeRows",
                type: "nvarchar(280)",
                maxLength: 280,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "ModerationReportedSnapshotRows",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
