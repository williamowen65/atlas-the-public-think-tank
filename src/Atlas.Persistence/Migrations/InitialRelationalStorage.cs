using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Atlas.Persistence.Migrations;

[DbContext(typeof(AtlasDataContext))]
[Migration("20260929180000_InitialRelationalStorage")]
public sealed class InitialRelationalStorage : Migration
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) => InitialRelationalModel.ConfigureSnapshot(modelBuilder);

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "NodeRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                Title = table.Column<string> (type: "nvarchar(450)", nullable: false),
                DescriptionId = table.Column<Guid> (type: "uniqueidentifier", nullable: true),
                TypeId = table.Column<Guid> (type: "uniqueidentifier", nullable: true),
                AuthorId = table.Column<Guid> (type: "uniqueidentifier", nullable: true),
                Status = table.Column<string> (type: "nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
            },
            constraints: table => table.PrimaryKey(
                "PK_NodeRows", row => row.Id));
        migrationBuilder.CreateTable(
            name: "NodeTypeRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                Name = table.Column<string> (type: "nvarchar(450)", nullable: false),
                Description = table.Column<string> (type: "nvarchar(max)", nullable: false),
                OwnerId = table.Column<string> (type: "nvarchar(max)", nullable: true),
                IsSystemDefined = table.Column<bool> (type: "bit", nullable: false),
                IsArchived = table.Column<bool> (type: "bit", nullable: false),
                AutoPluralize = table.Column<bool> (type: "bit", nullable: true),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
            },
            constraints: table => table.PrimaryKey(
                "PK_NodeTypeRows", row => row.Id));
        migrationBuilder.CreateTable(
            name: "DocumentRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
            },
            constraints: table => table.PrimaryKey(
                "PK_DocumentRows", row => row.Id));
        migrationBuilder.CreateTable(
            name: "BlockRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                Kind = table.Column<string> (type: "nvarchar(450)", nullable: false),
                Markdown = table.Column<string> (type: "nvarchar(max)", nullable: true),
                AltText = table.Column<string> (type: "nvarchar(max)", nullable: true),
                Caption = table.Column<string> (type: "nvarchar(max)", nullable: true),
                Url = table.Column<string> (type: "nvarchar(max)", nullable: true),
                Title = table.Column<string> (type: "nvarchar(max)", nullable: true),
                Description = table.Column<string> (type: "nvarchar(max)", nullable: true),
                ReferenceId = table.Column<Guid> (type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
            },
            constraints: table => table.PrimaryKey(
                "PK_BlockRows", row => row.Id));
        migrationBuilder.CreateTable(
            name: "ParticipantRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                DisplayName = table.Column<string> (type: "nvarchar(max)", nullable: false),
                Bio = table.Column<string> (type: "nvarchar(max)", nullable: false),
                IsActive = table.Column<bool> (type: "bit", nullable: false),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
            },
            constraints: table => table.PrimaryKey(
                "PK_ParticipantRows", row => row.Id));
        migrationBuilder.CreateTable(
            name: "VoteRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                ParticipantId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                TargetId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                TargetType = table.Column<string> (type: "nvarchar(450)", nullable: false),
                Value = table.Column<int> (type: "int", nullable: false),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
            },
            constraints: table => table.PrimaryKey(
                "PK_VoteRows", row => row.Id));
        migrationBuilder.CreateIndex("IX_Vote_UniqueParticipantTarget", "VoteRows", new[] { "ParticipantId", "TargetType", "TargetId" }, unique: true);
        migrationBuilder.CreateTable(
            name: "ReactionDefinitionRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                Text = table.Column<string> (type: "nvarchar(max)", nullable: false),
                Emoji = table.Column<string> (type: "nvarchar(max)", nullable: false),
                Description = table.Column<string> (type: "nvarchar(max)", nullable: false),
                NormalizedText = table.Column<string> (type: "nvarchar(450)", nullable: false),
                CreatedByParticipantId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                IsSuppressed = table.Column<bool> (type: "bit", nullable: false),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
            },
            constraints: table => table.PrimaryKey(
                "PK_ReactionDefinitionRows", row => row.Id));
        migrationBuilder.CreateTable(
            name: "NodeReactionRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                NodeId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                ReactionDefinitionId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                AppliedByParticipantId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                LifecycleState = table.Column<string> (type: "nvarchar(450)", nullable: false),
                Disposition = table.Column<string> (type: "nvarchar(max)", nullable: false),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                RemovedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: true),
            },
            constraints: table => table.PrimaryKey(
                "PK_NodeReactionRows", row => row.Id));
        migrationBuilder.CreateTable(
            name: "CommunityRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                Name = table.Column<string> (type: "nvarchar(450)", nullable: false),
                Description = table.Column<string> (type: "nvarchar(max)", nullable: false),
                OwnerParticipantId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                Status = table.Column<string> (type: "nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
            },
            constraints: table => table.PrimaryKey(
                "PK_CommunityRows", row => row.Id));
        migrationBuilder.CreateIndex("IX_Community_Name", "CommunityRows", "Name", unique: true);
        migrationBuilder.CreateTable(
            name: "CommunityMembershipRows",
            columns: table => new
            {
                CommunityId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                ParticipantId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                JoinedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                LeftAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: true),
            },
            constraints: table => table.PrimaryKey(
                "PK_CommunityMembershipRows", row => new { row.CommunityId, row.ParticipantId }));
        migrationBuilder.CreateTable(
            name: "CommunityNodeRows",
            columns: table => new
            {
                CommunityId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                NodeId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                AssociatedByParticipantId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                AssociatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
            },
            constraints: table => table.PrimaryKey(
                "PK_CommunityNodeRows", row => new { row.CommunityId, row.NodeId }));
        migrationBuilder.CreateTable(
            name: "CommentRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                TargetKind = table.Column<string> (type: "nvarchar(450)", nullable: false),
                TargetId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                ParentCommentId = table.Column<Guid> (type: "uniqueidentifier", nullable: true),
                AuthorParticipantId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                Body = table.Column<string> (type: "nvarchar(max)", nullable: false),
                Status = table.Column<string> (type: "nvarchar(450)", nullable: false),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                RemovedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: true),
            },
            constraints: table => table.PrimaryKey(
                "PK_CommentRows", row => row.Id));
        migrationBuilder.CreateTable(
            name: "ModerationCaseRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                NodeId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                ReporterId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                Reason = table.Column<string> (type: "nvarchar(max)", nullable: false),
                Explanation = table.Column<string> (type: "nvarchar(max)", nullable: true),
                ReportedTitle = table.Column<string> (type: "nvarchar(max)", nullable: false),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                Status = table.Column<int> (type: "int", nullable: false),
                ReviewerId = table.Column<Guid> (type: "uniqueidentifier", nullable: true),
                DecisionReason = table.Column<string> (type: "nvarchar(max)", nullable: true),
                DecidedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: true),
                PublicReason = table.Column<int> (type: "int", nullable: false),
                ReviewRequestedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: true),
                VisibilityRestoredAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: true),
                RestoredBy = table.Column<Guid> (type: "uniqueidentifier", nullable: true),
                RestorationReason = table.Column<string> (type: "nvarchar(max)", nullable: true),
            },
            constraints: table => table.PrimaryKey(
                "PK_ModerationCaseRows", row => row.Id));
        migrationBuilder.CreateTable(
            name: "NotificationRows",
            columns: table => new
            {
                Id = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                OccurrenceId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                RecipientParticipantId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                Category = table.Column<int> (type: "int", nullable: false),
                InAppVisible = table.Column<bool> (type: "bit", nullable: false),
                Kind = table.Column<string> (type: "nvarchar(450)", nullable: false),
                SubjectKind = table.Column<string> (type: "nvarchar(450)", nullable: false),
                SubjectId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                ActorParticipantId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                CreatedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: false),
                ReadAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: true),
                DismissedAt = table.Column<DateTimeOffset> (type: "datetimeoffset", nullable: true),
            },
            constraints: table => table.PrimaryKey(
                "PK_NotificationRows", row => row.Id));
        migrationBuilder.CreateIndex("IX_Notification_UniqueOccurrenceRecipient", "NotificationRows", new[] { "OccurrenceId", "RecipientParticipantId" }, unique: true);
        migrationBuilder.CreateTable(
            name: "NotificationPreferencesRows",
            columns: table => new
            {
                ParticipantId = table.Column<Guid> (type: "uniqueidentifier", nullable: false),
                DiscussionInApp = table.Column<bool> (type: "bit", nullable: false),
                ModerationInApp = table.Column<bool> (type: "bit", nullable: false),
                DiscussionEmail = table.Column<bool> (type: "bit", nullable: false),
                ModerationEmail = table.Column<bool> (type: "bit", nullable: false),
                DiscussionPush = table.Column<bool> (type: "bit", nullable: false),
                ModerationPush = table.Column<bool> (type: "bit", nullable: false),
            },
            constraints: table => table.PrimaryKey(
                "PK_NotificationPreferencesRows", row => row.ParticipantId));
        migrationBuilder.CreateTable(
            name: "NodeParents",
            columns: table => new
            {
                NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Position = table.Column<int>(type: "int", nullable: false),
                ParentNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NodeParents", row => new { row.NodeId, row.Position });
                table.ForeignKey("FK_NodeParents_NodeRows", row => row.NodeId, "NodeRows", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateTable(
            name: "NodeRequestedTypes",
            columns: table => new
            {
                NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Position = table.Column<int>(type: "int", nullable: false),
                TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NodeRequestedTypes", row => new { row.NodeId, row.Position });
                table.ForeignKey("FK_NodeRequestedTypes_NodeRows", row => row.NodeId, "NodeRows", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateTable(
            name: "DocumentBlocks",
            columns: table => new
            {
                DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Position = table.Column<int>(type: "int", nullable: false),
                BlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentBlocks", row => new { row.DocumentId, row.Position });
                table.ForeignKey("FK_DocumentBlocks_DocumentRows", row => row.DocumentId, "DocumentRows", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateTable(
            name: "ReactionAuditEntries",
            columns: table => new
            {
                NodeReactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Position = table.Column<int>(type: "int", nullable: false),
                Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ActorParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LifecycleState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Disposition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RelatedNodeReactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReactionAuditEntries", row => new { row.NodeReactionId, row.Position });
                table.ForeignKey("FK_ReactionAuditEntries_NodeReactionRows", row => row.NodeReactionId, "NodeReactionRows", "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateTable(
            name: "NotificationDeliveryAttempts",
            columns: table => new
            {
                NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Position = table.Column<int>(type: "int", nullable: false),
                Channel = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                AttemptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NotificationDeliveryAttempts", row => new { row.NotificationId, row.Position });
                table.ForeignKey("FK_NotificationDeliveryAttempts_NotificationRows", row => row.NotificationId, "NotificationRows", "Id", onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("NotificationDeliveryAttempts");
        migrationBuilder.DropTable("ReactionAuditEntries");
        migrationBuilder.DropTable("DocumentBlocks");
        migrationBuilder.DropTable("NodeRequestedTypes");
        migrationBuilder.DropTable("NodeParents");

        migrationBuilder.DropTable("NotificationPreferencesRows");
        migrationBuilder.DropTable("NotificationRows");
        migrationBuilder.DropTable("ModerationCaseRows");
        migrationBuilder.DropTable("CommentRows");
        migrationBuilder.DropTable("CommunityNodeRows");
        migrationBuilder.DropTable("CommunityMembershipRows");
        migrationBuilder.DropTable("CommunityRows");
        migrationBuilder.DropTable("NodeReactionRows");
        migrationBuilder.DropTable("ReactionDefinitionRows");
        migrationBuilder.DropTable("VoteRows");
        migrationBuilder.DropTable("ParticipantRows");
        migrationBuilder.DropTable("BlockRows");
        migrationBuilder.DropTable("DocumentRows");
        migrationBuilder.DropTable("NodeTypeRows");
        migrationBuilder.DropTable("NodeRows");
    }
}
