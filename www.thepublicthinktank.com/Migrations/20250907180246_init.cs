using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedContentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user2@example.com", true, true, null, "SEED.USER2@EXAMPLE.COM", "SEED.USER.TWO", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Two" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user3@example.com", true, true, null, "SEED.USER3@EXAMPLE.COM", "SEED.USER.THREE", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Three" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user4@example.com", true, true, null, "SEED.USER4@EXAMPLE.COM", "SEED.USER.FOUR", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Four" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user5@example.com", true, true, null, "SEED.USER5@EXAMPLE.COM", "SEED.USER.FIVE", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Five" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user6@example.com", true, true, null, "SEED.USER6@EXAMPLE.COM", "SEED.USER.SIX", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Six" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user7@example.com", true, true, null, "SEED.USER7@EXAMPLE.COM", "SEED.USER.SEVEN", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Seven" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user8@example.com", true, true, null, "SEED.USER8@EXAMPLE.COM", "SEED.USER.EIGHT", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Eight" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user9@example.com", true, true, null, "SEED.USER9@EXAMPLE.COM", "SEED.USER.NINE", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Nine" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user10@example.com", true, true, null, "SEED.USER10@EXAMPLE.COM", "SEED.USER.TEN", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Ten" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user11@example.com", true, true, null, "SEED.USER11@EXAMPLE.COM", "SEED.USER.ELEVEN", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Eleven" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user12@example.com", true, true, null, "SEED.USER12@EXAMPLE.COM", "SEED.USER.TWELVE", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Twelve" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user13@example.com", true, true, null, "SEED.USER13@EXAMPLE.COM", "SEED.USER.THIRTEEN", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Thirteen" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user14@example.com", true, true, null, "SEED.USER14@EXAMPLE.COM", "SEED.USER.FOURTEEN", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Fourteen" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user15@example.com", true, true, null, "SEED.USER15@EXAMPLE.COM", "SEED.USER.FIFTEEN", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Fifteen" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee45"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user16@example.com", true, true, null, "SEED.USER16@EXAMPLE.COM", "SEED.USER.SIXTEEN", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Sixteen" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee46"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user17@example.com", true, true, null, "SEED.USER17@EXAMPLE.COM", "SEED.USER.SEVENTEEN", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Seventeen" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee47"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user18@example.com", true, true, null, "SEED.USER18@EXAMPLE.COM", "SEED.USER.EIGHTEEN", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Eighteen" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee48"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user19@example.com", true, true, null, "SEED.USER19@EXAMPLE.COM", "SEED.USER.NINETEEN", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Nineteen" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee49"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user20@example.com", true, true, null, "SEED.USER20@EXAMPLE.COM", "SEED.USER.TWENTY", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.Twenty" },
                    { new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "seed.user1@example.com", true, true, null, "SEED.USER1@EXAMPLE.COM", "SEED.USER.ONE", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Seed.User.One" }
                });

            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[,]
                {
                    { new Guid("96bb172c-23fc-4359-b324-e221db3682a9"), "[2,1]", "[3]", "[1,2,0]", "[3]", "[0,1]" },
                    { new Guid("b2e2e2c7-7e2a-4e2d-9b1a-2c3e4f5a6b7c"), "[1,2]", "[1,0,2,4,5]", "[0,1,2]", "[3,4,5]", "[1]" }
                });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "SolutionID", "Title" },
                values: new object[] { new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), null, "Homelessness remains a pervasive and complex crisis affecting individuals, families, and entire communities across urban and rural areas alike. Driven by a combination of factors—including unaffordable housing, poverty, unemployment, mental health challenges, substance use disorders, and systemic inequality—homelessness not only strips individuals of stability and dignity but also places strain on public services and local economies.\n\nMarginalized populations, such as veterans, LGBTQ+ youth, people of color, and those exiting foster care or incarceration, are disproportionately impacted. Despite numerous policy efforts, shelters remain overcrowded, permanent housing solutions underfunded, and preventive measures insufficient.\n\nTackling homelessness requires a coordinated, compassionate approach that addresses both immediate needs and the root causes of housing instability.", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, new Guid("b2e2e2c7-7e2a-4e2d-9b1a-2c3e4f5a6b7c"), null, "Homelessness" });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("0d1e2f3a-4b5c-6d7e-8f9a-0b1c2d3e4f5a"), null, new DateTime(2024, 1, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), null, new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 10 },
                    { new Guid("1e2f3a4b-5c6d-7e8f-9a0b-1c2d3e4f5a6b"), null, new DateTime(2024, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), null, new DateTime(2024, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("2f3a4b5c-6d7e-8f9a-0b1c-2d3e4f5a6b7c"), null, new DateTime(2024, 1, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("3a4b5c6d-7e8f-9a0b-1c2d-3e4f5a6b7c8d"), null, new DateTime(2024, 1, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 7 },
                    { new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), null, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 8 },
                    { new Guid("4b5c6d7e-8f9a-0b1c-2d3e-4f5a6b7c8d9e"), null, new DateTime(2024, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), null, new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 7 },
                    { new Guid("5c6d7e8f-9a0b-1c2d-3e4f-5a6b7c8d9e0f"), null, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 10 },
                    { new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), null, new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 9 },
                    { new Guid("6d7e8f9a-0b1c-2d3e-4f5a-6b7c8d9e0f1a"), null, new DateTime(2024, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee45"), 8 },
                    { new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), null, new DateTime(2024, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 10 },
                    { new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"), null, new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 8 },
                    { new Guid("7e8f9a0b-1c2d-3e4f-5a6b-7c8d9e0f1a2b"), null, new DateTime(2024, 2, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee46"), 7 },
                    { new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), null, new DateTime(2024, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 7 },
                    { new Guid("8f9a0b1c-2d3e-4f5a-6b7c-8d9e0f1a2b3c"), null, new DateTime(2024, 2, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee47"), 9 },
                    { new Guid("9a0b1c2d-3e4f-5a6b-7c8d-9e0f1a2b3c4d"), null, new DateTime(2024, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee48"), 8 },
                    { new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"), null, new DateTime(2024, 1, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 6 }
                });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), null, "Mobile Outreach Teams with Clinicians and Social Workers represent a proactive, relationship-based approach to engaging people experiencing homelessness who may be disconnected from traditional service systems. By bringing multidisciplinary expertise directly to individuals where they live—whether in encampments, vehicles, abandoned buildings, or other unsheltered locations—these teams establish trust, provide immediate assistance, and create pathways to housing, healthcare, and long-term support.\n\nEffective mobile outreach teams typically include several key professionals working in coordination: Licensed clinicians (psychiatrists, psychiatric nurse practitioners, or clinical social workers) who can conduct field-based mental health and substance use assessments, provide brief interventions, prescribe medications when appropriate, and facilitate connections to ongoing treatment; Social workers or case managers who assist with benefits applications, housing navigation, and coordination of various services; Peer support specialists with lived experience of homelessness who offer authentic connection, practical guidance, and hope through their own recovery journeys; and occasionally, specially trained law enforcement officers or emergency medical technicians who can address safety concerns or medical emergencies with a humanitarian, rather than punitive, approach.\n\nThe operational model emphasizes consistency, persistence, and respect for individual autonomy. Teams visit the same locations on predictable schedules, allowing for relationship development over time. They practice trauma-informed engagement, recognizing that many homeless individuals have experienced past traumatic events, including negative interactions with service systems. Rather than requiring immediate compliance with program expectations, teams work at the individual's pace, beginning with low-barrier assistance that addresses immediate needs—food, hygiene supplies, wound care, harm reduction supplies—while gradually building trust for more intensive interventions.\n\nMobile outreach teams are equipped with technology and resources that enable field-based service delivery: Tablets or laptops with cellular connectivity for real-time documentation, benefits applications, and housing registries; Transportation capacity to accompany clients to appointments; Basic medical supplies for first aid and health assessments; Emergency funds for immediate needs like temporary accommodations or identification documents; and Direct access to shelter beds or transitional housing units reserved specifically for outreach referrals, allowing teams to offer immediate alternatives to unsheltered homelessness.\n\nWhen implemented effectively, mobile outreach yields significant benefits: Improved engagement of highly vulnerable individuals who would otherwise remain disconnected from services; Reduced reliance on costly emergency systems like hospitals and jails; Earlier intervention in health and mental health conditions before they reach crisis levels; More successful housing placements due to the trust established through consistent outreach; and Improved community relations by addressing visible homelessness with compassion rather than criminalization.\n\nSuccessful implementation requires dedicated funding for competitive salaries, appropriate staffing ratios, quality supervision, and comprehensive training. Programs must balance geographic coverage with sufficient time for meaningful engagement, avoid becoming merely a crisis response system, and maintain strong connections to housing resources to ensure outreach leads to permanent solutions rather than merely managing homelessness.", 1, new DateTime(2024, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), new Guid("96bb172c-23fc-4359-b324-e221db3682a9"), "Mobile Outreach Teams with Clinicians and Social Workers" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a1f0e9d8-7c64-41b5-b309-4e82f7a5b1c0"), null, new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("a3f2e1d0-9c86-43b7-b521-6e04f9a7b3c2"), null, new DateTime(2024, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("a7f6e5d4-3c20-47b1-b965-0e48f3a1b7c6"), null, new DateTime(2024, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 9 },
                    { new Guid("b2a1f0e9-8d75-42c6-b410-5f93a8b6c2d1"), null, new DateTime(2024, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("b4a3f2e1-0d97-44c8-b632-7f15a0b8c4d3"), null, new DateTime(2024, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 10 },
                    { new Guid("b8a7f6e5-4d31-48c2-b076-1f59a4b2c8d7"), null, new DateTime(2024, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 8 },
                    { new Guid("c3b2a1f0-9e86-43d7-b521-6a04b9c7d3e2"), null, new DateTime(2024, 10, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("c9b8a7f6-5e42-49d3-b187-2a60b5c3d9e8"), null, new DateTime(2024, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("d0c9b8a7-6f53-40e4-b298-3b71c6d4e0f9"), null, new DateTime(2024, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("d4c3b2a1-0f97-44e8-a632-7b15c0d8e4f3"), null, new DateTime(2024, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("e1d0c9b8-7a64-41f5-b309-4c82d7e5f1a0"), null, new DateTime(2024, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("e5d4c3b2-1a08-45f9-b743-8c26d1e9f5a4"), null, new DateTime(2024, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("f0e9d8c7-6b53-40a4-b298-3d71e6f4a0b9"), null, new DateTime(2024, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 10 },
                    { new Guid("f2e1d0c9-8b75-42a6-b410-5d93e8f6a2b1"), null, new DateTime(2024, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 8 },
                    { new Guid("f6e5d4c3-2b19-46a0-b854-9d37e2f0a6b5"), null, new DateTime(2024, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 }
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
