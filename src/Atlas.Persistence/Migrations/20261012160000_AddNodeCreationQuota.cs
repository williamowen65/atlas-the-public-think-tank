using Microsoft.EntityFrameworkCore.Migrations;

namespace Atlas.Persistence.Migrations;

public partial class AddNodeCreationQuota : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "NodeCreationQuotaRows",
            columns: table => new
            {
                NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_NodeCreationQuotaRows", row => row.NodeId));
        migrationBuilder.CreateIndex(
            name: "IX_NodeCreationQuotaRows_ParticipantId_CreatedAt",
            table: "NodeCreationQuotaRows",
            columns: new[] { "ParticipantId", "CreatedAt" });
        // Existing authored Nodes count immediately. Old rows naturally fall outside the
        // rolling window; historical creations already physically deleted cannot be recovered.
        migrationBuilder.Sql("INSERT INTO NodeCreationQuotaRows (NodeId, ParticipantId, CreatedAt) SELECT Id, AuthorId, CreatedAt FROM NodeRows WHERE AuthorId IS NOT NULL;");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "NodeCreationQuotaRows");
}
