using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "app");

            migrationBuilder.EnsureSchema(
                name: "comments");

            migrationBuilder.EnsureSchema(
                name: "issues");

            migrationBuilder.EnsureSchema(
                name: "scopes");

            migrationBuilder.EnsureSchema(
                name: "solutions");

            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
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
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlockedContent",
                schema: "app",
                columns: table => new
                {
                    BlockedContentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReasonID = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedContent", x => x.BlockedContentID);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                schema: "scopes",
                columns: table => new
                {
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Scales = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Domains = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timeframes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Boundaries = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.ScopeID);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ScopesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "scopes")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
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
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
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
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                schema: "comments",
                columns: table => new
                {
                    CommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    ParentCommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    AuthorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedContentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentID);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_BlockedContent_BlockedContentID",
                        column: x => x.BlockedContentID,
                        principalSchema: "app",
                        principalTable: "BlockedContent",
                        principalColumn: "BlockedContentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentID",
                        column: x => x.ParentCommentID,
                        principalSchema: "comments",
                        principalTable: "Comments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "CommentsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "comments")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "CommentVotes",
                schema: "comments",
                columns: table => new
                {
                    VoteID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoteValue = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentVotes", x => x.VoteID);
                    table.CheckConstraint("CK_CommentVote_VoteValue_Range", "[VoteValue] >= 0 AND [VoteValue] <= 10");
                    table.ForeignKey(
                        name: "FK_CommentVotes_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommentVotes_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommentVotes_Comments_CommentID",
                        column: x => x.CommentID,
                        principalSchema: "comments",
                        principalTable: "Comments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                schema: "issues",
                columns: table => new
                {
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentIssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentSolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    AuthorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedContentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RANK = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.IssueID);
                    table.ForeignKey(
                        name: "FK_Issues_AspNetUsers_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_BlockedContent_BlockedContentID",
                        column: x => x.BlockedContentID,
                        principalSchema: "app",
                        principalTable: "BlockedContent",
                        principalColumn: "BlockedContentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Issues_ParentIssueID",
                        column: x => x.ParentIssueID,
                        principalSchema: "issues",
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Scopes_ScopeID",
                        column: x => x.ScopeID,
                        principalSchema: "scopes",
                        principalTable: "Scopes",
                        principalColumn: "ScopeID");
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "IssuesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "issues")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "IssuesTags",
                schema: "issues",
                columns: table => new
                {
                    TagID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuesTags", x => new { x.TagID, x.IssueID });
                    table.ForeignKey(
                        name: "FK_IssuesTags_Issues_IssueID",
                        column: x => x.IssueID,
                        principalSchema: "issues",
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueVotes",
                schema: "issues",
                columns: table => new
                {
                    VoteID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoteValue = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueVotes", x => x.VoteID);
                    table.CheckConstraint("CK_IssueVote_VoteValue_Range", "[VoteValue] >= 0 AND [VoteValue] <= 10");
                    table.ForeignKey(
                        name: "FK_IssueVotes_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IssueVotes_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueVotes_Issues_IssueID",
                        column: x => x.IssueID,
                        principalSchema: "issues",
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solutions",
                schema: "solutions",
                columns: table => new
                {
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentIssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    AuthorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedContentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RANK = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solutions", x => x.SolutionID);
                    table.ForeignKey(
                        name: "FK_Solutions_AspNetUsers_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solutions_BlockedContent_BlockedContentID",
                        column: x => x.BlockedContentID,
                        principalSchema: "app",
                        principalTable: "BlockedContent",
                        principalColumn: "BlockedContentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solutions_Issues_ParentIssueID",
                        column: x => x.ParentIssueID,
                        principalSchema: "issues",
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solutions_Scopes_ScopeID",
                        column: x => x.ScopeID,
                        principalSchema: "scopes",
                        principalTable: "Scopes",
                        principalColumn: "ScopeID");
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "SolutionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "solutions")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "SolutionsTags",
                schema: "solutions",
                columns: table => new
                {
                    TagID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolutionsTags", x => new { x.TagID, x.SolutionID });
                    table.ForeignKey(
                        name: "FK_SolutionsTags_Solutions_SolutionID",
                        column: x => x.SolutionID,
                        principalSchema: "solutions",
                        principalTable: "Solutions",
                        principalColumn: "SolutionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolutionVotes",
                schema: "solutions",
                columns: table => new
                {
                    VoteID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoteValue = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolutionVotes", x => x.VoteID);
                    table.CheckConstraint("CK_SolutionVote_VoteValue_Range", "[VoteValue] >= 0 AND [VoteValue] <= 10");
                    table.ForeignKey(
                        name: "FK_SolutionVotes_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolutionVotes_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolutionVotes_Solutions_SolutionID",
                        column: x => x.SolutionID,
                        principalSchema: "solutions",
                        principalTable: "Solutions",
                        principalColumn: "SolutionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserHistory",
                schema: "users",
                columns: table => new
                {
                    UserHistoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHistory", x => x.UserHistoryID);
                    table.ForeignKey(
                        name: "FK_UserHistory_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserHistory_Comments_CommentID",
                        column: x => x.CommentID,
                        principalSchema: "comments",
                        principalTable: "Comments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserHistory_Issues_IssueID",
                        column: x => x.IssueID,
                        principalSchema: "issues",
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserHistory_Solutions_SolutionID",
                        column: x => x.SolutionID,
                        principalSchema: "solutions",
                        principalTable: "Solutions",
                        principalColumn: "SolutionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorID",
                schema: "comments",
                table: "Comments",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_BlockedContentID",
                schema: "comments",
                table: "Comments",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IssueID",
                schema: "comments",
                table: "Comments",
                column: "IssueID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentID",
                schema: "comments",
                table: "Comments",
                column: "ParentCommentID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_SolutionID",
                schema: "comments",
                table: "Comments",
                column: "SolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_CommentVotes_AppUserId",
                schema: "comments",
                table: "CommentVotes",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentVotes_CommentID_UserID",
                schema: "comments",
                table: "CommentVotes",
                columns: new[] { "CommentID", "UserID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentVotes_UserID",
                schema: "comments",
                table: "CommentVotes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_AuthorID",
                schema: "issues",
                table: "Issues",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_BlockedContentID",
                schema: "issues",
                table: "Issues",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ParentIssueID",
                schema: "issues",
                table: "Issues",
                column: "ParentIssueID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ParentSolutionID",
                schema: "issues",
                table: "Issues",
                column: "ParentSolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ScopeID",
                schema: "issues",
                table: "Issues",
                column: "ScopeID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_SolutionID",
                schema: "issues",
                table: "Issues",
                column: "SolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuesTags_IssueID",
                schema: "issues",
                table: "IssuesTags",
                column: "IssueID");

            migrationBuilder.CreateIndex(
                name: "IX_IssueVotes_AppUserId",
                schema: "issues",
                table: "IssueVotes",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueVotes_IssueID_UserID",
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "IssueID", "UserID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IssueVotes_UserID",
                schema: "issues",
                table: "IssueVotes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_AuthorID",
                schema: "solutions",
                table: "Solutions",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_BlockedContentID",
                schema: "solutions",
                table: "Solutions",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_ParentIssueID",
                schema: "solutions",
                table: "Solutions",
                column: "ParentIssueID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_ScopeID",
                schema: "solutions",
                table: "Solutions",
                column: "ScopeID");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionsTags_SolutionID",
                schema: "solutions",
                table: "SolutionsTags",
                column: "SolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionVotes_AppUserId",
                schema: "solutions",
                table: "SolutionVotes",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionVotes_SolutionID_UserID",
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "SolutionID", "UserID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolutionVotes_UserID",
                schema: "solutions",
                table: "SolutionVotes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_CommentID",
                schema: "users",
                table: "UserHistory",
                column: "CommentID");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_IssueID",
                schema: "users",
                table: "UserHistory",
                column: "IssueID");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_SolutionID",
                schema: "users",
                table: "UserHistory",
                column: "SolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_UserID",
                schema: "users",
                table: "UserHistory",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Issues_IssueID",
                schema: "comments",
                table: "Comments",
                column: "IssueID",
                principalSchema: "issues",
                principalTable: "Issues",
                principalColumn: "IssueID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Solutions_SolutionID",
                schema: "comments",
                table: "Comments",
                column: "SolutionID",
                principalSchema: "solutions",
                principalTable: "Solutions",
                principalColumn: "SolutionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Solutions_ParentSolutionID",
                schema: "issues",
                table: "Issues",
                column: "ParentSolutionID",
                principalSchema: "solutions",
                principalTable: "Solutions",
                principalColumn: "SolutionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Solutions_SolutionID",
                schema: "issues",
                table: "Issues",
                column: "SolutionID",
                principalSchema: "solutions",
                principalTable: "Solutions",
                principalColumn: "SolutionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_AspNetUsers_AuthorID",
                schema: "issues",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_AspNetUsers_AuthorID",
                schema: "solutions",
                table: "Solutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_BlockedContent_BlockedContentID",
                schema: "issues",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_BlockedContent_BlockedContentID",
                schema: "solutions",
                table: "Solutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_Issues_ParentIssueID",
                schema: "solutions",
                table: "Solutions");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CommentVotes",
                schema: "comments");

            migrationBuilder.DropTable(
                name: "IssuesTags",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "IssueVotes",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "SolutionsTags",
                schema: "solutions");

            migrationBuilder.DropTable(
                name: "SolutionVotes",
                schema: "solutions");

            migrationBuilder.DropTable(
                name: "UserHistory",
                schema: "users");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Comments",
                schema: "comments")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "CommentsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "comments")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BlockedContent",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Issues",
                schema: "issues")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "IssuesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "issues")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "Solutions",
                schema: "solutions")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "SolutionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "solutions")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "Scopes",
                schema: "scopes")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ScopesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "scopes")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");
        }
    }
}
