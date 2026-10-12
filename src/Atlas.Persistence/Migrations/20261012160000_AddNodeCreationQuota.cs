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
                NodeId = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false
                ),
                ParticipantId = table.Column<Guid>(
                    type: "uniqueidentifier",
                    nullable: false
                ),
                CreatedAt = table.Column<DateTimeOffset>(
                    type: "datetimeoffset",
                    nullable: false
                )
            },
            constraints: table => table.PrimaryKey(
                "PK_NodeCreationQuotaRows",
                row => row.NodeId
            )
        );
        migrationBuilder.CreateIndex(
            name: "IX_NodeCreationQuotaRows_ParticipantId_CreatedAt",
            table: "NodeCreationQuotaRows",
            columns: new[] {
                "ParticipantId",
                "CreatedAt"
            }
        );
        // Start accounting from rollout. Existing Nodes do not consume user slots.

    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "NodeCreationQuotaRows");
}
