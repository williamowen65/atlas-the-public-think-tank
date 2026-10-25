using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAtlasBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "AuditRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ObservabilityOperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlockRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Markdown = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Latin1_General_100_CI_AI_SC"),
                    AltText = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Latin1_General_100_CI_AI_SC"),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Latin1_General_100_CI_AI_SC"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Latin1_General_100_CI_AI_SC"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true, collation: "Latin1_General_100_CI_AI_SC"),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SearchText = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "CONVERT(nvarchar(max), CASE [Kind]\r\n    WHEN N'markdown' THEN COALESCE([Markdown], N'')\r\n    WHEN N'image' THEN CONCAT([AltText], N' ', [Caption])\r\n    WHEN N'video' THEN CONCAT([Caption], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'link-preview' THEN CONCAT([Title], N' ', [Description], N' ', [Url] COLLATE Latin1_General_100_CI_AI_SC)\r\n    WHEN N'chart-reference' THEN COALESCE([Title], N'')\r\n    ELSE N'' END)", stored: true, collation: "Latin1_General_100_CI_AI_SC")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunityCreationQuotaRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityCreationQuotaRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscoveryRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PageVersion = table.Column<long>(type: "bigint", nullable: false),
                    DetailVersion = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscoveryRevisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NodeCreationQuotaRows",
                columns: table => new
                {
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeCreationQuotaRows", x => x.NodeId);
                });

            migrationBuilder.CreateTable(
                name: "NodeTypeRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false, collation: "Latin1_General_100_CI_AS_SC"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSystemDefined = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    AutoPluralize = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeTypeRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportSubmissionQuotaRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSubmissionQuotaRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VoteActionQuotaRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteActionQuotaRows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LeaseOwner = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LeaseUntilUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditOutbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditOutbox_AuditRecords_AuditRecordId",
                        column: x => x.AuditRecordId,
                        principalTable: "AuditRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentBlocks",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    BlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentBlocks", x => new { x.DocumentId, x.Position });
                    table.ForeignKey(
                        name: "FK_DocumentBlocks_BlockRows_BlockId",
                        column: x => x.BlockId,
                        principalTable: "BlockRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentBlocks_DocumentRows_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "DocumentRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetKind = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RemovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentRows_CommentRows_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "CommentRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommentRows_ParticipantRows_AuthorParticipantId",
                        column: x => x.AuthorParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CommunityRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityRows_ParticipantRows_OwnerParticipantId",
                        column: x => x.OwnerParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NodeRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false, collation: "Latin1_General_100_CI_AI_SC"),
                    ShortDescription = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: false, defaultValue: ""),
                    DescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeRows_DocumentRows_DescriptionId",
                        column: x => x.DescriptionId,
                        principalTable: "DocumentRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NodeRows_NodeTypeRows_TypeId",
                        column: x => x.TypeId,
                        principalTable: "NodeTypeRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NodeRows_ParticipantRows_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferencesRows",
                columns: table => new
                {
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscussionInApp = table.Column<bool>(type: "bit", nullable: false),
                    ModerationInApp = table.Column<bool>(type: "bit", nullable: false),
                    DiscussionEmail = table.Column<bool>(type: "bit", nullable: false),
                    ModerationEmail = table.Column<bool>(type: "bit", nullable: false),
                    DiscussionPush = table.Column<bool>(type: "bit", nullable: false),
                    ModerationPush = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferencesRows", x => x.ParticipantId);
                    table.ForeignKey(
                        name: "FK_NotificationPreferencesRows_ParticipantRows_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotificationRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurrenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    InAppVisible = table.Column<bool>(type: "bit", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SubjectKind = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DismissedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationRows_ParticipantRows_ActorParticipantId",
                        column: x => x.ActorParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationRows_ParticipantRows_RecipientParticipantId",
                        column: x => x.RecipientParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReactionDefinitionRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NormalizedText = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedByParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsSuppressed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactionDefinitionRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReactionDefinitionRows_ParticipantRows_CreatedByParticipantId",
                        column: x => x.CreatedByParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_ParticipantRows_Id",
                        column: x => x.Id,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VoteRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoteRows_ParticipantRows_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunityMembershipRows",
                columns: table => new
                {
                    CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LeftAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityMembershipRows", x => new { x.CommunityId, x.ParticipantId });
                    table.ForeignKey(
                        name: "FK_CommunityMembershipRows_CommunityRows_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "CommunityRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommunityMembershipRows_ParticipantRows_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CommunityNodeRows",
                columns: table => new
                {
                    CommunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssociatedByParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssociatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityNodeRows", x => new { x.CommunityId, x.NodeId });
                    table.ForeignKey(
                        name: "FK_CommunityNodeRows_CommunityRows_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "CommunityRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommunityNodeRows_NodeRows_NodeId",
                        column: x => x.NodeId,
                        principalTable: "NodeRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommunityNodeRows_ParticipantRows_AssociatedByParticipantId",
                        column: x => x.AssociatedByParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ModerationCaseRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReporterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportedTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DecisionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PublicReason = table.Column<int>(type: "int", nullable: false),
                    ReviewRequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    VisibilityRestoredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RestoredBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RestorationReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationCaseRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationCaseRows_NodeRows_NodeId",
                        column: x => x.NodeId,
                        principalTable: "NodeRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ModerationCaseRows_ParticipantRows_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ModerationCaseRows_ParticipantRows_RestoredBy",
                        column: x => x.RestoredBy,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ModerationCaseRows_ParticipantRows_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NodeParents",
                columns: table => new
                {
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    ParentNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeParents", x => new { x.NodeId, x.Position });
                    table.ForeignKey(
                        name: "FK_NodeParents_NodeRows_NodeId",
                        column: x => x.NodeId,
                        principalTable: "NodeRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeParents_NodeRows_ParentNodeId",
                        column: x => x.ParentNodeId,
                        principalTable: "NodeRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NodeRequestedTypes",
                columns: table => new
                {
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeRequestedTypes", x => new { x.NodeId, x.Position });
                    table.ForeignKey(
                        name: "FK_NodeRequestedTypes_NodeRows_NodeId",
                        column: x => x.NodeId,
                        principalTable: "NodeRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeRequestedTypes_NodeTypeRows_TypeId",
                        column: x => x.TypeId,
                        principalTable: "NodeTypeRows",
                        principalColumn: "Id");
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
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDeliveryAttempts", x => new { x.NotificationId, x.Position });
                    table.ForeignKey(
                        name: "FK_NotificationDeliveryAttempts_NotificationRows_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "NotificationRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodeReactionRows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReactionDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppliedByParticipantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LifecycleState = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Disposition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RemovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeReactionRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeReactionRows_NodeRows_NodeId",
                        column: x => x.NodeId,
                        principalTable: "NodeRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NodeReactionRows_ParticipantRows_AppliedByParticipantId",
                        column: x => x.AppliedByParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NodeReactionRows_ReactionDefinitionRows_ReactionDefinitionId",
                        column: x => x.ReactionDefinitionId,
                        principalTable: "ReactionDefinitionRows",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                schema: "identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    RelatedNodeReactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReactionAuditEntries", x => new { x.NodeReactionId, x.Position });
                    table.ForeignKey(
                        name: "FK_ReactionAuditEntries_NodeReactionRows_NodeReactionId",
                        column: x => x.NodeReactionId,
                        principalTable: "NodeReactionRows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReactionAuditEntries_NodeReactionRows_RelatedNodeReactionId",
                        column: x => x.RelatedNodeReactionId,
                        principalTable: "NodeReactionRows",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReactionAuditEntries_ParticipantRows_ActorParticipantId",
                        column: x => x.ActorParticipantId,
                        principalTable: "ParticipantRows",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "DiscoveryRevisions",
                columns: new[] { "Id", "DetailVersion", "PageVersion" },
                values: new object[] { 1, 0L, 0L });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("94000000-0000-4000-8000-000000000001"), "94000000-0000-4000-8000-000000000001", "Member", "MEMBER" },
                    { new Guid("94000000-0000-4000-8000-000000000002"), "94000000-0000-4000-8000-000000000002", "GlobalModerator", "GLOBALMODERATOR" },
                    { new Guid("94000000-0000-4000-8000-000000000003"), "94000000-0000-4000-8000-000000000003", "Administrator", "ADMINISTRATOR" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditOutbox_AuditRecordId_Destination",
                table: "AuditOutbox",
                columns: new[] { "AuditRecordId", "Destination" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditOutbox_Destination_DeliveredAtUtc_NextAttemptAtUtc",
                table: "AuditOutbox",
                columns: new[] { "Destination", "DeliveredAtUtc", "NextAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_ObservabilityOperationId",
                table: "AuditRecords",
                column: "ObservabilityOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_TargetId_OccurredAtUtc",
                table: "AuditRecords",
                columns: new[] { "TargetId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CommentRows_AuthorParticipantId",
                table: "CommentRows",
                column: "AuthorParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentRows_ParentCommentId",
                table: "CommentRows",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentRows_SiblingPage",
                table: "CommentRows",
                columns: new[] { "TargetKind", "TargetId", "ParentCommentId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityCreationQuotaRows_ParticipantId_OccurredAt",
                table: "CommunityCreationQuotaRows",
                columns: new[] { "ParticipantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembershipRows_ParticipantId",
                table: "CommunityMembershipRows",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityNodeRows_AssociatedByParticipantId",
                table: "CommunityNodeRows",
                column: "AssociatedByParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityNodeRows_NodeId",
                table: "CommunityNodeRows",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Community_Name",
                table: "CommunityRows",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityRows_OwnerParticipantId",
                table: "CommunityRows",
                column: "OwnerParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentBlocks_BlockId",
                table: "DocumentBlocks",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCaseRows_NodeId",
                table: "ModerationCaseRows",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCaseRows_NodeId_Status_VisibilityRestoredAt",
                table: "ModerationCaseRows",
                columns: new[] { "NodeId", "Status", "VisibilityRestoredAt" })
                .Annotation("SqlServer:Include", new[] { "PublicReason", "DecidedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCaseRows_ReporterId",
                table: "ModerationCaseRows",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCaseRows_RestoredBy",
                table: "ModerationCaseRows",
                column: "RestoredBy");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCaseRows_ReviewerId",
                table: "ModerationCaseRows",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeCreationQuotaRows_ParticipantId_CreatedAt",
                table: "NodeCreationQuotaRows",
                columns: new[] { "ParticipantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeParents_ParentNodeId",
                table: "NodeParents",
                column: "ParentNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeReactionRows_AppliedByParticipantId",
                table: "NodeReactionRows",
                column: "AppliedByParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeReactionRows_NodeId",
                table: "NodeReactionRows",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeReactionRows_NodeId_LifecycleState_ReactionDefinitionId",
                table: "NodeReactionRows",
                columns: new[] { "NodeId", "LifecycleState", "ReactionDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeReactionRows_ReactionDefinitionId",
                table: "NodeReactionRows",
                column: "ReactionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeRequestedTypes_TypeId",
                table: "NodeRequestedTypes",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeRows_AuthorId",
                table: "NodeRows",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeRows_DescriptionId",
                table: "NodeRows",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeRows_TypeId",
                table: "NodeRows",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTypeRows_Name",
                table: "NodeTypeRows",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UniqueOccurrenceRecipient",
                table: "NotificationRows",
                columns: new[] { "OccurrenceId", "RecipientParticipantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRows_ActorParticipantId",
                table: "NotificationRows",
                column: "ActorParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRows_RecipientParticipantId",
                table: "NotificationRows",
                column: "RecipientParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionAuditEntries_ActorParticipantId",
                table: "ReactionAuditEntries",
                column: "ActorParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionAuditEntries_RelatedNodeReactionId",
                table: "ReactionAuditEntries",
                column: "RelatedNodeReactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionDefinitionRows_CreatedByParticipantId",
                table: "ReactionDefinitionRows",
                column: "CreatedByParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSubmissionQuotaRows_ParticipantId_OccurredAt",
                table: "ReportSubmissionQuotaRows",
                columns: new[] { "ParticipantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                schema: "identity",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "identity",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                schema: "identity",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                schema: "identity",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "identity",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "identity",
                table: "Users",
                column: "NormalizedEmail",
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identity",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VoteActionQuotaRows_ParticipantId_OccurredAt",
                table: "VoteActionQuotaRows",
                columns: new[] { "ParticipantId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Vote_UniqueParticipantTarget",
                table: "VoteRows",
                columns: new[] { "ParticipantId", "TargetType", "TargetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoteRows_ParticipantId",
                table: "VoteRows",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_VoteRows_TargetType_TargetId",
                table: "VoteRows",
                columns: new[] { "TargetType", "TargetId" })
                .Annotation("SqlServer:Include", new[] { "Value" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditOutbox");

            migrationBuilder.DropTable(
                name: "CommentRows");

            migrationBuilder.DropTable(
                name: "CommunityCreationQuotaRows");

            migrationBuilder.DropTable(
                name: "CommunityMembershipRows");

            migrationBuilder.DropTable(
                name: "CommunityNodeRows");

            migrationBuilder.DropTable(
                name: "DiscoveryRevisions");

            migrationBuilder.DropTable(
                name: "DocumentBlocks");

            migrationBuilder.DropTable(
                name: "ModerationCaseRows");

            migrationBuilder.DropTable(
                name: "NodeCreationQuotaRows");

            migrationBuilder.DropTable(
                name: "NodeParents");

            migrationBuilder.DropTable(
                name: "NodeRequestedTypes");

            migrationBuilder.DropTable(
                name: "NotificationDeliveryAttempts");

            migrationBuilder.DropTable(
                name: "NotificationPreferencesRows");

            migrationBuilder.DropTable(
                name: "ReactionAuditEntries");

            migrationBuilder.DropTable(
                name: "ReportSubmissionQuotaRows");

            migrationBuilder.DropTable(
                name: "RoleClaims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserClaims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserLogins",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserTokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "VoteActionQuotaRows");

            migrationBuilder.DropTable(
                name: "VoteRows");

            migrationBuilder.DropTable(
                name: "AuditRecords");

            migrationBuilder.DropTable(
                name: "CommunityRows");

            migrationBuilder.DropTable(
                name: "BlockRows");

            migrationBuilder.DropTable(
                name: "NotificationRows");

            migrationBuilder.DropTable(
                name: "NodeReactionRows");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "NodeRows");

            migrationBuilder.DropTable(
                name: "ReactionDefinitionRows");

            migrationBuilder.DropTable(
                name: "DocumentRows");

            migrationBuilder.DropTable(
                name: "NodeTypeRows");

            migrationBuilder.DropTable(
                name: "ParticipantRows");
        }
    }
}
