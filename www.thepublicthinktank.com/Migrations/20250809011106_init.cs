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
                name: "Categories",
                schema: "app",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Scopes",
                schema: "app",
                columns: table => new
                {
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scopes", x => x.ScopeID);
                });

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
                    Comment = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    ParentCommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                });

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
                columns: table => new
                {
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentIssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentSolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    AuthorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedContentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Scopes_ScopeID",
                        column: x => x.ScopeID,
                        principalSchema: "app",
                        principalTable: "Scopes",
                        principalColumn: "ScopeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IssuesCategories",
                schema: "issues",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuesCategories", x => new { x.CategoryID, x.IssueID });
                    table.ForeignKey(
                        name: "FK_IssuesCategories_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalSchema: "app",
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssuesCategories_Issues_IssueID",
                        column: x => x.IssueID,
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
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
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solutions",
                columns: table => new
                {
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentIssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    AuthorID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedContentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solutions_Scopes_ScopeID",
                        column: x => x.ScopeID,
                        principalSchema: "app",
                        principalTable: "Scopes",
                        principalColumn: "ScopeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolutionsCategories",
                schema: "solutions",
                columns: table => new
                {
                    CategoryID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolutionsCategories", x => new { x.CategoryID, x.SolutionID });
                    table.ForeignKey(
                        name: "FK_SolutionsCategories_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalSchema: "app",
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolutionsCategories_Solutions_SolutionID",
                        column: x => x.SolutionID,
                        principalTable: "Solutions",
                        principalColumn: "SolutionID",
                        onDelete: ReferentialAction.Restrict);
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
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserVoteID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
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
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserHistory_Solutions_SolutionID",
                        column: x => x.SolutionID,
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
                schema: "app",
                table: "Scopes",
                columns: new[] { "ScopeID", "ScopeName" },
                values: new object[,]
                {
                    { new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Global" },
                    { new Guid("b2d8f1a7-e4c9-4f8a-8d5f-7e6c9d8b3a2f"), "National" },
                    { new Guid("c3b9a2d8-f1e7-4f8a-9c3b-8d7e6f5d4c2b"), "Local" },
                    { new Guid("d4c9b3a2-f8e7-4f8a-9c3b-8d7e6f5d4c2b"), "Individual" }
                });

            migrationBuilder.InsertData(
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), null, "Rapid advancements in artificial intelligence, robotics, and automation are fundamentally reshaping the global workforce, creating both unprecedented opportunities and significant challenges. As machines increasingly perform tasks traditionally done by humans, from manufacturing and customer service to data analysis and medical diagnostics, millions of workers face potential displacement and uncertain futures.\n\nThis technological revolution disproportionately impacts certain sectors and demographics, particularly routine-based occupations and workers without advanced education or specialized skills. While automation creates new types of jobs, there's growing concern about whether enough new positions will emerge to compensate for those eliminated, and whether displaced workers can successfully transition to these new roles.\n\nThe social and economic implications extend beyond individual livelihoods to affect entire communities, potentially widening inequality and creating social instability. How can we harness the productivity and innovation of automation while ensuring its benefits are broadly shared? What policies, educational reforms, and social safety nets might help workers navigate this shifting landscape? And how do we balance technological progress with human dignity and economic security?\n\nThese questions demand urgent attention from policymakers, business leaders, educators, and citizens as we collectively shape how automation will transform not just our economy, but the very nature of work itself.", 1, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Workforce Automation and Job Displacement" },
                    { new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "As these platforms become integral to how people connect, communicate, and access information, many challenges persist that raise critical questions. How can social media companies improve transparency around their content moderation policies to ensure fairness and consistency? Are their algorithms designed in ways that prioritize user well-being over engagement and profit?\n\nWhat responsibilities do social media sites have in combating misinformation, hate speech, and harmful content without infringing on free expression? How can they better protect user privacy and data security amid growing concerns over surveillance and misuse?\n\nMoreover, how might social media platforms address the mental health impacts linked to prolonged use, especially among young and vulnerable populations? And importantly, how can they create safer, more inclusive online communities where harassment and abuse are minimized?\n\nThese questions point to deep systemic issues in the design, governance, and business models of social media platforms. Addressing them is essential for building digital spaces that truly support healthy public discourse, individual rights, and social cohesion.", 1, new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Can Social Media Platforms Be Better?" },
                    { new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), null, "Homelessness remains a pervasive and complex crisis affecting individuals, families, and entire communities across urban and rural areas alike. Driven by a combination of factors—including unaffordable housing, poverty, unemployment, mental health challenges, substance use disorders, and systemic inequality—homelessness not only strips individuals of stability and dignity but also places strain on public services and local economies.\n\nMarginalized populations, such as veterans, LGBTQ+ youth, people of color, and those exiting foster care or incarceration, are disproportionately impacted. Despite numerous policy efforts, shelters remain overcrowded, permanent housing solutions underfunded, and preventive measures insufficient.\n\nTackling homelessness requires a coordinated, compassionate approach that addresses both immediate needs and the root causes of housing instability.", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Homelessness" },
                    { new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Climate change is no longer a distant threat—it is a present-day crisis reshaping our planet in real time. Driven primarily by the burning of fossil fuels, deforestation, and unsustainable land use, climate change is increasing global temperatures, disrupting weather patterns, and accelerating the frequency and intensity of natural disasters.\n\nThe consequences are wide-reaching: rising sea levels threaten coastal communities, prolonged droughts endanger food and water supplies, and extreme heat waves place vulnerable populations at serious risk. Ecosystems are under immense stress, with species extinction accelerating as habitats are lost or altered beyond recovery.\n\nAt its core, the issue is not just environmental—it is also social, economic, and moral. Climate change disproportionately affects those who contribute the least to it: low-income communities, indigenous populations, and developing nations often lack the resources to adapt or recover. Without urgent, coordinated global action, these inequalities will deepen, and the window to prevent irreversible damage will continue to close.\n\nTo confront this crisis, we must dramatically reduce greenhouse gas emissions, invest in renewable energy, protect natural ecosystems, and build resilient infrastructure. The challenge is immense, but so is the responsibility—and the opportunity—to shape a livable future for all.", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Climate Change Solutions" }
                });

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
                    { new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"), null, new DateTime(2024, 1, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 6 },
                    { new Guid("a1b24f8c-3e12-47d6-9e78-5f98c732a641"), null, new DateTime(2024, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("a1b27e90-c864-45d8-e61f-4d2a0b81b65c"), null, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 7 },
                    { new Guid("a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), null, new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("a3b49e12-c086-47de-ec3f-6d4a2b03b87c"), null, new DateTime(2024, 1, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee46"), 9 },
                    { new Guid("a6b78c9d-0e12-4f34-a556-8b90c123d974"), null, new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 10 },
                    { new Guid("a6b7c8d9-e012-4f34-a567-8a90b123c207"), null, new DateTime(2024, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("a6b7c8d9-e012-4f34-ab56-7a90b123c863"), null, new DateTime(2024, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 1 },
                    { new Guid("a7b83e56-c420-41de-ec7f-0d8a6b47b21c"), null, new DateTime(2024, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 7 },
                    { new Guid("a7b8c9d0-e123-4a45-bc67-8d90e123f752"), null, new DateTime(2024, 1, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 2 },
                    { new Guid("a7b8c9d0-e1f2-3a4b-5c6d-7e8f9a0b1c2d"), null, new DateTime(2024, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("a9a8b7c6-d543-4e21-fa09-8b76c543d530"), null, new DateTime(2024, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 7 },
                    { new Guid("b2c38f01-d975-46e9-f72a-5e3b1c92c76d"), null, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("b2c3d4e5-f6a7-8b9c-0d1e-2f3a4b5c6d7e"), null, new DateTime(2024, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 7 },
                    { new Guid("b3c45d9e-5f21-48e7-af89-6f07d843b752"), null, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 9 },
                    { new Guid("b4c50f23-d197-48ef-fd4a-7e5b3c14c98d"), null, new DateTime(2024, 1, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee47"), 8 },
                    { new Guid("b8c94f67-d531-42ef-fd8a-1e9b7c58c32d"), null, new DateTime(2024, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("b8c9d0e1-f2a3-4b5c-6d7e-8f9a0b1c2d3e"), null, new DateTime(2024, 1, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 6 },
                    { new Guid("c1d2e3f4-a567-4b89-cd01-2e34f567a429"), null, new DateTime(2024, 1, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 3 },
                    { new Guid("c3d49a12-e086-47fa-a83b-6f4c2d03d87e"), null, new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 8 },
                    { new Guid("c3d4e5f6-a7b8-9c0d-1e2f-3a4b5c6d7e8f"), null, new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 6 },
                    { new Guid("c5d61a34-e208-49fa-ae5b-8f6c4d25d09e"), null, new DateTime(2024, 1, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee48"), 10 },
                    { new Guid("c7d58e3a-9f21-47b6-a12d-8e44bc67f531"), null, new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 10 },
                    { new Guid("c7d58e3a-9f21-47b6-a12d-8e94bc67f531"), null, new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 10 },
                    { new Guid("c9d05a78-e642-43fa-ae9b-2f0c8d69d43e"), null, new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 10 },
                    { new Guid("c9d0e1f2-3a45-4b67-cd89-0e12f345a196"), null, new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 6 },
                    { new Guid("d0e16b89-f753-44ab-bf0c-3a1d9e70e54f"), null, new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("d4e50b23-f197-48ab-b94c-7a5d3e14e98f"), null, new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 6 },
                    { new Guid("d4e5f6a7-b890-4c12-de34-5f67a890b318"), null, new DateTime(2024, 1, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 4 },
                    { new Guid("d4e5f6a7-b8c9-0d1e-2f3a-4b5c6d7e8f9a"), null, new DateTime(2024, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("d5e67f0a-7b23-49c8-bd90-7e18f934c863"), null, new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 7 },
                    { new Guid("d6e72b45-f319-40ab-bf6c-9a7d5e36e10f"), null, new DateTime(2024, 1, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee49"), 9 },
                    { new Guid("d8e94b67-f531-42a5-b38c-1a9d7e58e32f"), null, new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("e1f27c90-a864-45bc-ca1d-4b2e0f81f65a"), null, new DateTime(2024, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 8 },
                    { new Guid("e1f2a3b4-5c67-4d89-ae01-2f34e567f085"), null, new DateTime(2024, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 5 },
                    { new Guid("e2f3a4b5-c678-4d90-ef12-3a45b678c641"), null, new DateTime(2024, 1, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("e5f61c34-a208-49bc-ca5d-8b6e4f25f09a"), null, new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 10 },
                    { new Guid("e5f6a7b8-c9d0-1e2f-3a4b-5c6d7e8f9a0b"), null, new DateTime(2024, 1, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 5 },
                    { new Guid("e7f8d9c0-a1b2-4c3d-9e8f-7a6b5c4d3e2f"), null, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("e9f05c78-a642-43b6-c49d-2b0e8f69f43a"), null, new DateTime(2024, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 8 },
                    { new Guid("f0a16d89-b753-44c7-d50e-3c1f9a70a54b"), null, new DateTime(2024, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 10 },
                    { new Guid("f2a38d01-b975-46cd-db2e-5c3f1a92a76b"), null, new DateTime(2024, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee45"), 7 },
                    { new Guid("f4a5b6c7-d890-4e12-af34-5a67b890c974"), null, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 6 },
                    { new Guid("f6a72d45-b319-40cd-db6e-9c7f5a36a10b"), null, new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("f6a7b8c9-d0e1-2f3a-4b5c-6d7e8f9a0b1c"), null, new DateTime(2024, 1, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("f8e9d0c1-b2a3-4d5e-0f1a-2b3c4d5e6f7a"), null, new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a9b8c7d6-e5f4-4321-b0a9-c8d7e6f5a4b3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 }
                });

            migrationBuilder.InsertData(
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), null, "Homelessness and housing instability are not just the result of individual circumstances, but often reflect deeper systemic failures and gaps in the social safety net. When institutions designed to protect vulnerable populations break down, individuals and families can quickly fall through the cracks, facing cycles of poverty, instability, and exclusion.\n\nKey failures include insufficient access to mental health care, addiction treatment, and preventive health services; inadequate unemployment insurance and income support; lack of affordable childcare; and fragmented or underfunded transitional services for those leaving foster care, prison, or military service.\n\nBureaucratic barriers, eligibility restrictions, and complex application processes often prevent those most in need from accessing help. Many safety net programs are reactive rather than proactive, intervening only after crises have escalated. Coordination between agencies is frequently poor, resulting in duplicated efforts, missed opportunities, and gaps in care.\n\nAddressing these systemic failures requires a holistic approach: investing in robust, accessible safety nets; streamlining service delivery; prioritizing prevention and early intervention; and ensuring that support systems are trauma-informed, culturally competent, and responsive to the needs of diverse populations. By strengthening the social safety net, we can reduce the risk of homelessness and promote greater stability and opportunity for all.", 1, new DateTime(2024, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Systemic Failures and Safety Nets" },
                    { new Guid("9a7e6cbd-8c1f-4d3f-a07f-3d8c9c2bfc44"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Ocean acidification is one of the less visible but most serious consequences of climate change. It occurs when the oceans absorb excess carbon dioxide (CO₂) from the atmosphere—a process that has accelerated rapidly due to human activities like burning fossil fuels and deforestation. When CO₂ dissolves in seawater, it forms carbonic acid, which lowers the ocean's pH. Since the start of the industrial era, the ocean has become approximately 30% more acidic, marking a significant shift in its chemistry over a relatively short period.\n\nThis acidification disrupts marine ecosystems, particularly affecting organisms that rely on calcium carbonate to build their shells and skeletons—such as corals, oysters, clams, and some plankton. As the pH drops, it becomes harder for these organisms to form and maintain their structures, leading to weaker shells and slower growth. Coral reefs, often referred to as the 'rainforests of the sea,' are especially vulnerable. Weakened coral skeletons make reef systems more prone to collapse, which threatens the biodiversity they support and the millions of people who depend on them for food, tourism, and coastal protection.\n\nThe effects ripple through the entire food chain. When small shell-forming organisms struggle to survive, larger animals that feed on them—like fish and whales—also face risks. This destabilization can lead to a decline in fish populations, which directly impacts global food security. Moreover, many communities, especially in island and coastal regions, rely heavily on healthy marine ecosystems for their livelihoods. Thus, ocean acidification is not just an environmental issue—it’s an economic and humanitarian one as well.", 1, new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Ocean Acidification" },
                    { new Guid("a5d9c7e8-3b2f-47a1-9c5e-8f6d4b2a1c3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "A major contributor to homelessness is the chronic shortage of affordable housing, particularly in high-demand urban areas. The supply of housing units that are both available and affordable to low-income individuals and families has not kept pace with demand, creating intense competition, long waitlists, and rising homelessness rates.\n\nKey challenges include:\n\nZoning restrictions and land-use policies that limit multi-family or low-income housing construction.\n\nHigh construction and land costs, especially in urban centers, which discourage affordable development.\n\nReduced federal and state investment in public and subsidized housing over recent decades.\n\nCommunity opposition (NIMBYism) that stalls or blocks housing projects aimed at lower-income populations.\n\nInsufficient incentives for private developers to build units affordable to extremely low-income tenants.\n\nWithout enough affordable housing, shelters and transitional programs are stretched thin, and individuals exiting homelessness often have nowhere to go. Solutions may include reforming zoning laws, expanding housing trust funds, increasing public-private partnerships, and scaling up supportive housing models.", 1, new DateTime(2024, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Affordable Housing" },
                    { new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), null, "Many major social media platforms operate on business models that prioritize user engagement and attention as the primary metrics for success. These models rely on advertising revenue, which increases when users spend more time on the platform and engage more frequently with content. This creates a fundamental misalignment between what's profitable for platforms and what's healthy for individuals and society.\n\nResearch consistently shows that emotionally charged content—particularly material that triggers outrage, fear, or divisiveness—generates significantly more engagement than neutral or positive content. Algorithms designed to maximize engagement therefore tend to amplify the most provocative and polarizing voices, regardless of accuracy or social value. This creates feedback loops where content creators are incentivized to produce increasingly extreme material to maintain visibility.\n\nThe consequences of this system are far-reaching. Public discourse becomes dominated by the most inflammatory perspectives rather than the most thoughtful ones. Complex issues are reduced to simplified, antagonistic narratives. Users are pushed toward increasingly radical content through recommendation systems. And social cohesion suffers as different groups are exposed to dramatically different information environments tailored to reinforce their existing views.\n\nSome argue that these outcomes aren't bugs but features of a system working exactly as designed—to capture and monetize human attention regardless of the social cost. Addressing this issue requires fundamentally reimagining the economic incentives that drive platform design, potentially through regulation, alternative business models, or both. Without such changes, platforms may continue optimizing for engagement metrics that fail to account for human and social well-being.", 1, new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Ad-Driven Models Incentivizing Outrage and Engagement At All Costs" },
                    { new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), null, "Social media platforms have fundamentally altered how political discourse unfolds, often intensifying political divisions and creating environments where extremist viewpoints can flourish. Several structural elements of these platforms contribute to this phenomenon, presenting challenges for democratic societies globally.\n\nRecommendation algorithms typically prioritize content that generates strong emotional reactions, including outrage and partisan anger. This creates feedback loops where increasingly extreme political content receives greater visibility and engagement, effectively rewarding polarization. Meanwhile, platform architecture often facilitates the formation of ideologically homogeneous communities where more moderate voices are marginalized and radical ideas become normalized through group dynamics and reinforcement.\n\nThe attention economy of these platforms also incentivizes politicians, media outlets, and content creators to adopt more extreme, divisive positions to maintain visibility and audience engagement. Complex policy discussions are reduced to inflammatory sound bites, and nuanced perspectives struggle to gain traction in an environment optimized for controversy rather than understanding.\n\nAdditionally, malicious actors—including some foreign governments—have exploited these platform vulnerabilities to intentionally amplify existing social divisions, often using sophisticated targeting techniques to reach receptive audiences with content designed to heighten tensions and undermine democratic discourse.\n\nAddressing these challenges requires examining the design choices that facilitate polarization and extremism, exploring alternative platform architectures that might foster healthier political discourse, and developing literacy around how these systems shape our understanding of political issues. Solutions must balance concerns about censorship and free expression against the need for information environments that support democratic values rather than undermine them.", 1, new DateTime(2024, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Amplification of Political Polarization and Extremism" },
                    { new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), null, "The landscape of digital public discourse has become increasingly concentrated in the hands of a few powerful technology companies. A small number of private corporations now own and control the primary platforms where billions of people communicate, share information, and form opinions on matters of public importance. This unprecedented centralization of communicative power raises profound questions for democratic societies.\n\nUnlike traditional media which operated under various public interest obligations, social media platforms function largely as private spaces governed by corporate terms of service rather than democratic principles. This means that critical decisions about acceptable speech, content moderation, and algorithmic amplification are made by executives accountable primarily to shareholders rather than citizens or elected representatives.\n\nThe consequences of this arrangement are far-reaching. Platform owners can unilaterally establish rules affecting billions of users across diverse cultural and political contexts. They can amplify or suppress certain types of content based on opaque algorithms. And they can implement sweeping policy changes with minimal external oversight or transparent justification.\n\nWhile these platforms have enabled unprecedented global connection and democratized content creation, the consolidation of control over our primary communication infrastructure in so few hands poses significant risks. Questions of platform monopoly power, alternative ownership models, and appropriate governance frameworks have become urgent as digital communications increasingly shape our public life and democratic processes. Finding the right balance between innovation, free expression, and democratic accountability remains one of the central challenges of our digital age.", 1, new DateTime(2024, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Centralized Ownership of Massive Public Discourse" },
                    { new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), null, "Social media platforms face an increasingly difficult balancing act between limiting harmful content and preserving freedom of expression. This tension represents one of the most fundamental challenges in digital governance today, with significant implications for public discourse, safety, and democracy.\n\nOn one side, platforms have a responsibility to address genuine harms that can occur in unmoderated spaces—including harassment, incitement to violence, exploitation of vulnerable groups, and coordinated disinformation campaigns. Research increasingly links unmoderated harmful content to real-world consequences, from psychological damage to political violence. Many users expect platforms to provide some level of protection against these harms.\n\nOn the other side, content moderation decisions inevitably involve subjective judgments about what constitutes harmful speech. Critics argue that excessive moderation risks removing legitimate political discourse, artistic expression, or marginalized voices. There are valid concerns about corporate entities having broad powers to determine acceptable speech, particularly given their global reach across diverse cultural contexts and political systems.\n\nThis dilemma is complicated by several factors: the massive scale of content requiring moderation; the limitations of automated systems in understanding context and nuance; the varying cultural and legal standards across different countries; and the financial incentives that may influence platform governance decisions. Disagreements about appropriate content policies often reflect deeper philosophical differences about the relative importance of harm prevention versus expressive freedom.\n\nFinding sustainable approaches to this challenge requires grappling with fundamental questions about the nature and limits of free expression in digital contexts. Should platforms be treated as public forums with minimal restrictions, or as curated spaces with clearer boundaries? How can moderation systems achieve greater transparency, consistency, and accountability? And what role should governments, civil society, and users themselves play in developing and implementing content governance frameworks that balance competing values?", 1, new DateTime(2024, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Censorship vs. Free Speech Tensions" },
                    { new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), null, "Housing markets across many regions are experiencing a profound and multifaceted crisis that extends far beyond homelessness to affect middle-income households, young adults, retirees, and virtually all segments of society. This crisis manifests in rapidly escalating home prices and rents that consistently outpace wage growth, creating a situation where housing costs consume an unsustainable portion of household incomes.\n\nAt the heart of this issue lies a fundamental supply-demand imbalance. Decades of underbuilding have resulted in housing shortages estimated in the millions of units. This undersupply stems from multiple factors working in tandem: restrictive zoning laws that prevent density and efficient land use; complex and lengthy approval processes that increase development costs and timelines; construction labor shortages; rising material costs; and significant barriers to scaling innovative building technologies.\n\nThe consequences of this crisis extend beyond housing itself. Economic mobility is hampered as workers cannot afford to live near job opportunities. Intergenerational wealth gaps widen as homeownership—historically a primary vehicle for middle-class wealth building—becomes increasingly inaccessible to younger generations. Environmental goals suffer as housing shortages in transit-rich urban areas push development to car-dependent exurbs, increasing commute times and carbon emissions.\n\nCommunities face additional challenges as essential workers—teachers, healthcare providers, first responders—are priced out of the areas they serve. Demographic shifts occur as families delay formation, aging adults cannot downsize appropriately, and diverse populations are displaced from established neighborhoods.\n\nAddressing this crisis requires coordinated efforts across multiple domains: land use reform to enable more housing production of varied types; investment in housing subsidies and affordable development; innovations in construction methods and financing models; tenant protections that maintain stability without discouraging supply growth; and regional approaches that recognize housing markets transcend municipal boundaries.", 1, new DateTime(2024, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Housing Supply and Affordability Crisis" },
                    { new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), null, "The rise of social media platforms has coincided with alarming trends in adolescent mental health and body image concerns. Today's teenagers are exposed to a constant stream of carefully curated, often digitally altered images that present unrealistic standards of beauty, success, and lifestyle. This digital environment has created unprecedented challenges for young people developing their sense of self and place in the world.\n\nResearch increasingly suggests connections between heavy social media use and increased rates of depression, anxiety, and body dissatisfaction among adolescents. The pressure to receive validation through likes and comments, constant comparison to peers and influencers, and the fear of missing out (FOMO) can create harmful psychological patterns that may persist into adulthood. Young women and LGBTQ+ youth appear particularly vulnerable to these negative effects.\n\nThe algorithmic amplification of content that drives engagement often prioritizes extreme, idealized, or controversial material, creating distorted perceptions of reality. Beauty filters and editing tools that alter appearances have become normalized, blurring the line between authentic and manufactured self-presentation.\n\nWhile social media platforms implement some safeguards, many argue these measures remain insufficient against the powerful commercial incentives driving user engagement. Addressing this issue requires coordinated efforts from technology companies, parents, educators, healthcare providers, and policymakers to create healthier digital environments that support rather than undermine adolescent development and well-being.", 1, new DateTime(2024, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Impact on Adolescent Mental Health and Body Image" },
                    { new Guid("e8f2a395-c16d-48c1-b31c-d7c5a622b2f5"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Across the globe, we are witnessing a dramatic and accelerating decline in biodiversity. Endangered species—animals, plants, and entire ecosystems—are disappearing at an alarming rate due to human activities such as habitat destruction, pollution, climate change, poaching, and the introduction of invasive species.\n\nThis crisis is not just about losing individual species—it is about the collapse of entire ecological networks. When keystone species vanish, food chains unravel, pollination fails, water systems destabilize, and the natural balance that supports life on Earth begins to erode. The loss of biodiversity undermines the health of ecosystems we all depend on—for clean air, fertile soil, stable climate, and even medical breakthroughs.\n\nThe issue is urgent and deeply systemic. Current extinction rates are estimated to be 1,000 times higher than the natural background rate, a pace not seen since the last mass extinction event. Yet, many species are disappearing silently, without ever being studied or even discovered.\n\nWithout immediate and sustained global action, we risk not only irreversible ecological damage but also profound consequences for human survival. Protecting endangered species means preserving the interconnected web of life. It demands stronger conservation laws, habitat restoration, indigenous land stewardship, and a commitment to shifting our relationship with nature—from exploitation to stewardship.", 1, new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Critical Decline of Endangered Species" },
                    { new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), null, "The digital information ecosystem has become increasingly vulnerable to the rapid spread of false or misleading content. Social media platforms, by design, can amplify misinformation at unprecedented speeds and scales, reaching millions of users before corrections can catch up. This creates a troubling dynamic where falsehoods often travel faster and reach wider audiences than verified facts.\n\nSimultaneously, personalization algorithms create 'filter bubbles' and 'echo chambers' that limit exposure to diverse viewpoints. These systems, designed to maximize engagement by showing users content similar to what they've previously interacted with, inadvertently reinforce existing beliefs and minimize contradictory information. Users become progressively isolated in information environments that reflect and amplify their existing views, making them more susceptible to misleading content that aligns with their preconceptions.\n\nThe combination of these factors has serious implications for democratic societies. Public discourse increasingly operates from divergent factual foundations, making consensus-building and collaborative problem-solving more difficult. Trust in institutions, expertise, and shared sources of information continues to erode. And heightened polarization driven by separate information realities threatens social cohesion and democratic functioning.\n\nAddressing this challenge requires multifaceted approaches involving platform design changes, media literacy initiatives, regulatory frameworks, and innovations in content verification. Finding solutions that balance free expression with information integrity remains one of the most urgent challenges in our digital media environment.", 1, new DateTime(2024, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Spread of Misinformation and Echo Chambers" },
                    { new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), null, "Social stigma remains one of the most persistent yet under-addressed barriers preventing individuals experiencing homelessness from seeking and accessing available support services. This stigmatization manifests in various forms that significantly impact both individual behavior and institutional responses.\n\nAt the individual level, stigma often leads to feelings of shame, embarrassment, and diminished self-worth among those experiencing homelessness. These emotional burdens can cause people to avoid seeking services, hide their housing status, or refuse to identify themselves as homeless—even when doing so would connect them with crucial resources. Many report fears of being judged, discriminated against, or treated disrespectfully when interacting with service providers, healthcare facilities, or government agencies.\n\nPublic misconceptions about homelessness frequently center on assumptions that it results primarily from personal failings rather than systemic issues like housing unaffordability, poverty, inadequate mental health care, and other structural factors. These misunderstandings further reinforce stigma and can lead to dehumanizing treatment of homeless individuals in public spaces and service settings.\n\nInstitutional practices often inadvertently perpetuate stigma through bureaucratic procedures, intrusive questioning, or service environments that lack dignity and privacy. Many homeless services operate from a deficit model that emphasizes compliance rather than empowerment, further alienating potential clients.\n\nAddressing stigma requires coordinated approaches, including public education campaigns, trauma-informed care training for service providers, peer support models that employ formerly homeless individuals, and service design that prioritizes dignity and self-determination. By tackling the invisible barrier of stigma, we can significantly improve service utilization and effectiveness in addressing homelessness.", 1, new DateTime(2024, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Stigma Preventing People From Seeking Help" }
                });

            migrationBuilder.InsertData(
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Atlas: The Public Think Tank represents a paradigm shift in how social media platforms function. While traditional platforms prioritize engagement metrics and advertising revenue, Atlas focuses on collaborative problem-solving and thoughtful discourse.\n\nKey innovations include:\n\n- Nuanced voting system: Instead of simplistic likes/dislikes, Atlas employs a 0-10 scale that encourages thoughtful evaluation of content quality and relevance\n\n- Issue-solution framework: Content is organized around problems and their potential solutions, creating natural context for constructive discussion\n\n- Transparency by design: Algorithm settings are fully adjustable by users, giving people control over what they see and why\n\n- Community-driven development: The platform itself is treated as an evolving project that users can help improve\n\nAtlas addresses many core problems with current social media: the amplification of divisive content, lack of nuance in discussions, and the prioritization of engagement over user wellbeing. By creating a space specifically designed for collaborative thinking and problem-solving, Atlas demonstrates that social platforms can be reimagined to better serve human needs.\n\nThis solution doesn't just critique existing social media—it offers a concrete alternative that shows how technology can be harnessed to connect people in more meaningful, productive ways.", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Atlas - The Public Think Tank" },
                    { new Guid("a2c7d49e-5f38-41b6-9e76-8c429d5b1f83"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Let users toggle what affects their feed: recency, variety, depth, etc.\n\nSocial media platforms should implement transparent algorithm settings that allow users to have direct control over what content appears in their feeds. This solution would enable individuals to customize their experience based on personal preferences and values, rather than being subject to black-box algorithms optimized solely for engagement metrics.\n\nKey features would include:\n\n- Adjustable content preferences: Users could set sliders for content recency vs. relevance, content diversity, topic depth, and the balance between content from connections versus broader sources\n\n- Explicit content filters: Clear options to filter sensitive topics, controversial content, or specific categories based on personal comfort levels\n\n- Algorithm transparency documentation: Plain-language explanations of how the algorithm works and what factors influence content selection\n\n- Usage insights: Data visualizations showing users how their content consumption patterns affect what they see\n\nThis approach would return agency to users, reduce algorithm-driven echo chambers, and build trust through transparency. Platforms implementing such settings would differentiate themselves as more ethical and user-centric, potentially attracting individuals concerned about digital wellbeing and algorithmic manipulation.", 1, new DateTime(2024, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Transparent Algorithm Settings" },
                    { new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Posts are made of blocks — Problem, Evidence, Opinion, Ask — to improve clarity.\n\nSocial media platforms should implement 'Modular Posting' as a structured content creation framework that breaks posts into distinct, labeled components. This solution would transform the standard free-form posting format into a more organized approach that helps both creators and readers distinguish between different types of information.\n\nKey modules would include:\n\n- Problem: A clearly defined issue or question being addressed\n\n- Evidence: Factual information, data, or sources supporting claims\n\n- Opinion: Clearly marked personal perspectives or interpretations\n\n- Ask: Specific calls to action, questions for discussion, or requests\n\nAdditional features would include:\n\n- Visual differentiation: Each module would have distinct styling to make the post structure immediately apparent to readers\n\n- Flexible ordering: Users could arrange modules in the sequence that best serves their communication goals\n\n- Optional modules: Not all posts would require all module types, allowing flexibility while maintaining structure\n\n- Advanced filtering: Readers could filter content based on module types (e.g., 'show me posts with evidence')\n\nThis approach would significantly improve content clarity by helping users distinguish between facts, opinions, and requests. It would encourage more thoughtful content creation by prompting users to consider different aspects of their communication. For readers, modular posts would enable faster comprehension and more effective evaluation of information. This structure would be particularly valuable for complex topics where mixing different types of information often leads to misunderstandings and unnecessary conflicts.", 1, new DateTime(2024, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Modular Posting" },
                    { new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Quietly shows users how long they've been passive vs. active contributors.\n\nSocial media platforms should implement a 'Consumption Tracker' that provides users with insights into their engagement patterns. This solution would offer a subtle but impactful way to increase awareness of passive consumption versus active participation on the platform.\n\nKey features would include:\n\n- Time visualization: An unobtrusive dashboard showing the balance between time spent scrolling/viewing versus time spent creating content, commenting, or otherwise contributing\n\n- Weekly insights: Gentle notifications or summaries that highlight engagement patterns without judgment, celebrating contribution milestones\n\n- Contribution diversity metrics: Tracking different types of engagement (e.g., original posts, thoughtful comments, sharing educational content) to recognize various forms of valuable contribution\n\n- Customizable goals: Optional, user-defined targets for desired participation levels that align with personal digital wellbeing objectives\n\n- Privacy controls: Full user control over tracking data, with options to pause tracking or delete history\n\nUnlike addictive engagement metrics, this solution emphasizes quality of engagement rather than quantity. By making users aware of their consumption-to-contribution ratio, platforms could encourage more mindful usage patterns and foster a community of active participants rather than passive consumers. This approach also empowers users to make informed decisions about their digital habits without employing manipulative design techniques.", 1, new DateTime(2024, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Consumption Tracker" },
                    { new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), null, "Second Chance Hiring Incentives represent a comprehensive approach to addressing employment barriers for individuals transitioning from homelessness, incarceration, or extended unemployment. These programs create pathways to stable employment—a critical factor in securing and maintaining housing.\n\nThe solution involves multi-faceted incentives for employers who hire qualified candidates with barriers to employment. Tax credits form the foundation, offering businesses direct financial benefits for each eligible employee hired and retained. Wage subsidies complement tax incentives by offsetting initial training costs during the critical onboarding period when productivity may be developing. Bonding programs provide insurance protection against potential employee dishonesty, removing a significant concern for employers considering candidates with criminal histories.\n\nBeyond financial incentives, this approach includes support services that benefit both employers and employees: specialized job coaches who provide ongoing mentorship; liaison services that help navigate workplace challenges; and training grants that fund skill development tailored to specific industry needs. Recognition programs highlight businesses demonstrating inclusive hiring practices, creating positive public relations opportunities.\n\nImplementation requires collaboration between government agencies, community organizations, and the business community. Streamlined application processes and clear eligibility guidelines are essential to encourage employer participation. Success metrics should track not only initial placements but long-term retention and career advancement.\n\nWhen properly structured, Second Chance Hiring Incentives create mutual benefits: employers gain motivated, loyal employees and financial advantages, while vulnerable individuals secure economic self-sufficiency and stable housing. Communities benefit from reduced homelessness, decreased recidivism, expanded tax bases, and the economic multiplier effects of increased employment.", 1, new DateTime(2024, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Second Chance Hiring Incentives for Employers" },
                    { new Guid("c5d2e3f2-b1a0-47c9-8d67-5f2e3b4a1d90"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), null, "Microgrants for Unhoused Entrepreneurs or Gig Workers represents an innovative approach to addressing homelessness by fostering economic self-sufficiency through small-scale entrepreneurship and gig economy participation. This solution recognizes that many individuals experiencing homelessness possess valuable skills, creativity, and drive that can be leveraged to generate income when traditional employment paths may be inaccessible.\n\nThe program provides modest financial grants (typically $500-$5,000) directly to qualifying unhoused individuals with viable business ideas or who need equipment and resources to participate in gig economy opportunities. These funds can be used for essential business expenses: purchasing equipment or tools, securing necessary licenses or certifications, accessing digital technology, acquiring inventory, or covering transportation costs to work sites.\n\nBeyond financial support, a comprehensive microgrant program includes complementary resources that maximize success potential: business skills training tailored to different entrepreneurial ventures; mentorship from established entrepreneurs in similar fields; assistance with digital literacy and online platform navigation; simplified accounting tools for financial management; and connections to community markets, online platforms, or local businesses where goods or services can be sold.\n\nImplementation requires thoughtful design: low-barrier application processes that don't exclude those without formal documentation; tiered funding levels that allow for growth as businesses develop; flexibility in eligible expenses to accommodate diverse business models; and staged disbursement tied to business milestones rather than rigid timelines.\n\nThe benefits extend beyond immediate income generation. Participants develop transferable skills, build credit and financial history, establish professional networks, and gain confidence and dignity through meaningful work. The resulting financial stability helps secure and maintain housing, while successful microenterprises can potentially grow to employ others facing similar circumstances, creating a positive ripple effect in communities.", 1, new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Microgrants for Unhoused Entrepreneurs or Gig Workers" },
                    { new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "People post as personas to explore ideas, not identities.\n\nSocial media platforms should implement 'Roleplay Threads' as dedicated spaces where users can temporarily adopt different perspectives through clearly marked personas. This solution would create safe environments for exploring diverse viewpoints without the social consequences of permanently associating those views with one's personal identity.\n\nKey features would include:\n\n- Transparent persona creation: Users could create temporary personas with clear labels indicating they're roleplay identities, not authentic personal accounts\n\n- Perspective-based discussions: Threads would be organized around specific topics where users explicitly adopt different philosophical, professional, or cultural perspectives\n\n- Guided facilitation tools: Prompts and frameworks to help users explore ideas fairly and thoroughly from multiple angles\n\n- Civil discourse enforcement: Special moderation rules designed specifically for roleplay spaces to maintain respectful exploration while allowing challenging conversations\n\n- Knowledge-building focus: Emphasis on collaborative learning rather than performance or personal branding\n\nThis approach would transform social media from a place of rigid identity performance to a laboratory for intellectual exploration and empathy development. By separating ideas from identities, platforms could foster more nuanced conversations about complex topics without the polarization that occurs when views become tied to personal brands. Roleplay Threads would provide a structured environment for users to practice perspective-taking and develop a deeper understanding of different worldviews.", 1, new DateTime(2024, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Roleplay Threads" },
                    { new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), null, "Paid Transitional Employment programs offer structured, time-limited work opportunities that provide real income, build skills, and create pathways to permanent employment for individuals experiencing homelessness. These initiatives recognize that stable employment is a critical component of housing security, while acknowledging that many unhoused individuals face significant barriers to entering the traditional workforce immediately.\n\nUrban clean-up initiatives represent one successful model, employing homeless or recently housed individuals to maintain public spaces, remove litter, abate graffiti, and beautify neighborhoods. These programs serve multiple purposes: providing meaningful work with immediate compensation, improving community environments, fostering positive interactions between homeless individuals and the broader community, and demonstrating participants' capabilities and work ethic to potential employers.\n\nBeyond urban clean-up, effective transitional employment models include: maintenance and restoration of public parks and trails; peer outreach and navigation services for other homeless individuals; food service in community kitchens; retail positions in social enterprise businesses; administrative support in nonprofit organizations; and environmental stewardship projects. The most successful programs carefully match positions to participants' existing skills and interests while providing opportunities to develop new capabilities.\n\nComprehensive programs incorporate several key elements: predictable schedules with flexible options to accommodate health needs and service appointments; graduated responsibility as participants build confidence and skills; regular compensation at fair wages, ideally with opportunities for wage progression; integrated support services including case management, housing assistance, and mental health resources; financial literacy training and banking access; job-readiness preparation such as resume building and interview skills; and explicit pathways to permanent employment through partnerships with local businesses, preferential hiring agreements, or supported job placement.\n\nTransitional employment initiatives require thoughtful design to avoid potential pitfalls such as creating dependency or perpetuating low-wage work. Programs should establish clear timelines and goals, ensure that participants receive genuine skill development rather than just busywork, maintain strong relationships with permanent employers, and provide ongoing support during transitions to unsubsidized employment.\n\nWhen properly implemented, paid transitional employment delivers significant returns on investment: participants gain income stability, work experience, and self-confidence; communities benefit from improved public spaces and reduced visible homelessness; employers access a prepared workforce; and public systems may realize cost savings through reduced reliance on emergency services, shelters, and other crisis interventions.", 1, new DateTime(2024, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Paid Transitional Employment Programs" },
                    { new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Users can mark up each other's posts with constructive inline comments.\n\nSocial media platforms should implement an 'Annotation Mode' that allows users to provide contextual, paragraph-specific feedback directly on content. This solution would transform standard commenting from a sequential list of reactions into a more nuanced system of collaborative engagement with specific parts of posts.\n\nKey features would include:\n\n- Inline annotation tools: Users could highlight specific text, images, or video segments and attach comments directly to those elements\n\n- Constructive guidance frameworks: Prompts encouraging specific types of feedback (e.g., asking clarifying questions, providing relevant sources, offering alternative perspectives)\n\n- Author control settings: Content creators could enable different levels of annotation privileges, from open public annotation to limited trusted circles\n\n- Quality filtering: Algorithms and community moderation to surface the most constructive annotations while minimizing low-quality or antagonistic responses\n\n- Contextual view options: Readers could toggle between viewing content with or without annotations, or filter by annotation type\n\nThis approach would transform social media interactions from performative posturing to collaborative knowledge building. By focusing on specific parts of content rather than generalized reactions, annotations would encourage more thoughtful engagement and reduce misunderstandings. Content creators would receive more useful feedback, and readers would benefit from additional context and perspective. The annotation layer would serve as a bridge between original content and discussion, creating a more interconnected and meaningful discourse environment.", 1, new DateTime(2024, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Annotation Mode" },
                    { new Guid("e7d9c6b2-3a5f-4182-9e08-51bd72af46c3"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "Let users filter feed by emotional tone — e.g., calm, curious, motivated.\n\nSocial media platforms should implement a 'Mood Bubbles' feature that allows users to curate their content experience based on desired emotional states. Unlike traditional content filters that focus on topics or sources, this solution would categorize content by its likely emotional impact on viewers.\n\nKey features would include:\n\n- Emotional tone filtering: Users could select from emotional states like 'calm', 'curious', 'motivated', 'inspired', 'joyful', or 'reflective' to match their current needs or desired mood\n\n- AI-powered content analysis: Content would be analyzed for emotional tone using natural language processing and image recognition to identify its likely emotional impact\n\n- Personalized calibration: The system would learn individual user responses to content over time, recognizing that different people may react differently to the same content\n\n- Mood scheduling: Users could set different emotional preferences for different times of day (e.g., 'motivated' in the morning, 'calm' in the evening)\n\nThis approach would transform social media from a source of unpredictable emotional stimuli to a tool for intentional emotional well-being. It acknowledges that content consumption affects mental state and gives users agency in managing this impact. Platforms adopting this feature would position themselves as leaders in supporting digital wellness and emotional intelligence.", 1, new DateTime(2024, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b7c9e5d3-4a2f-48b1-9e7c-5d3a4b2f8c1e"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Mood Bubbles" },
                    { new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), null, "Mobile Outreach Teams with Clinicians and Social Workers represent a proactive, relationship-based approach to engaging people experiencing homelessness who may be disconnected from traditional service systems. By bringing multidisciplinary expertise directly to individuals where they live—whether in encampments, vehicles, abandoned buildings, or other unsheltered locations—these teams establish trust, provide immediate assistance, and create pathways to housing, healthcare, and long-term support.\n\nEffective mobile outreach teams typically include several key professionals working in coordination: Licensed clinicians (psychiatrists, psychiatric nurse practitioners, or clinical social workers) who can conduct field-based mental health and substance use assessments, provide brief interventions, prescribe medications when appropriate, and facilitate connections to ongoing treatment; Social workers or case managers who assist with benefits applications, housing navigation, and coordination of various services; Peer support specialists with lived experience of homelessness who offer authentic connection, practical guidance, and hope through their own recovery journeys; and occasionally, specially trained law enforcement officers or emergency medical technicians who can address safety concerns or medical emergencies with a humanitarian, rather than punitive, approach.\n\nThe operational model emphasizes consistency, persistence, and respect for individual autonomy. Teams visit the same locations on predictable schedules, allowing for relationship development over time. They practice trauma-informed engagement, recognizing that many homeless individuals have experienced past traumatic events, including negative interactions with service systems. Rather than requiring immediate compliance with program expectations, teams work at the individual's pace, beginning with low-barrier assistance that addresses immediate needs—food, hygiene supplies, wound care, harm reduction supplies—while gradually building trust for more intensive interventions.\n\nMobile outreach teams are equipped with technology and resources that enable field-based service delivery: Tablets or laptops with cellular connectivity for real-time documentation, benefits applications, and housing registries; Transportation capacity to accompany clients to appointments; Basic medical supplies for first aid and health assessments; Emergency funds for immediate needs like temporary accommodations or identification documents; and Direct access to shelter beds or transitional housing units reserved specifically for outreach referrals, allowing teams to offer immediate alternatives to unsheltered homelessness.\n\nWhen implemented effectively, mobile outreach yields significant benefits: Improved engagement of highly vulnerable individuals who would otherwise remain disconnected from services; Reduced reliance on costly emergency systems like hospitals and jails; Earlier intervention in health and mental health conditions before they reach crisis levels; More successful housing placements due to the trust established through consistent outreach; and Improved community relations by addressing visible homelessness with compassion rather than criminalization.\n\nSuccessful implementation requires dedicated funding for competitive salaries, appropriate staffing ratios, quality supervision, and comprehensive training. Programs must balance geographic coverage with sufficient time for meaningful engagement, avoid becoming merely a crisis response system, and maintain strong connections to housing resources to ensure outreach leads to permanent solutions rather than merely managing homelessness.", 1, new DateTime(2024, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Mobile Outreach Teams with Clinicians and Social Workers" }
                });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("0a1b2c3d-4e5f-46ab-c8d9-0e1f2a3b4c5d"), null, new DateTime(2024, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 7 },
                    { new Guid("0b7c6d5e-4f8a-4b9c-1d2e-3f4a5b6c7d8e"), null, new DateTime(2024, 2, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 10 },
                    { new Guid("0c1d2e3f-4a5b-46c7-8d9e-0f1a2b3c4d5e"), null, new DateTime(2024, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 10 },
                    { new Guid("0d7e6f5a-4b8c-4d9e-1f2a-3b4c5d6e7f8a"), null, new DateTime(2024, 2, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("0e1f2a3b-4c5d-46e7-f8a9-0b1c2d3e4f5a"), null, new DateTime(2024, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 10 },
                    { new Guid("0f1a2b3c-4d5e-46f7-b8c9-0d1e2f3a4b5c"), null, new DateTime(2024, 3, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("1a2b3c4d-5e6f-47a8-c9d0-1e2f3a4b5c6d"), null, new DateTime(2024, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("1b2c3d4e-5f6a-47bc-d90e-1f2a3b4c5d6e"), null, new DateTime(2024, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 9 },
                    { new Guid("1c8d7e6f-5a9b-4c0d-2e3f-4a5b6c7d8e9f"), null, new DateTime(2024, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("1e8f7a6b-5c9d-4e0f-2a3b-4c5d6e7f8a9b"), null, new DateTime(2024, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 9 },
                    { new Guid("1f2a3b4c-5d6e-47f8-a90b-1c2d3e4f5a6b"), null, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 10 },
                    { new Guid("1f8a3c54-09be-47d6-a2c7-835fb940d6e9"), null, new DateTime(2024, 8, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 8 },
                    { new Guid("2a3b4c5d-6e7f-48a9-0b1c-2d3e4f5a6b7c"), null, new DateTime(2024, 5, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("2b3c4d5e-6f7a-48b9-d0e1-2f3a4b5c6d7e"), null, new DateTime(2024, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("2b7c1e49-5ad3-4f06-98e2-0c31fb57a8d4"), null, new DateTime(2024, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 7 },
                    { new Guid("2c3d4e5f-6a7b-48cd-0e1f-2a3b4c5d6e7f"), null, new DateTime(2024, 4, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("2d3e4f5a-6b7c-48d9-9e0f-1a2b3c4d5e6f"), null, new DateTime(2024, 3, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("2f9a8b7c-6d0e-4f1a-3b4c-5d6e7f8a9b0c"), null, new DateTime(2024, 2, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 7 },
                    { new Guid("3a0b9c8d-7e1f-4a2b-4c5d-6e7f8a9b0c1d"), null, new DateTime(2024, 2, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("3b4c5d6e-7f8a-490b-1c2d-3e4f5a6b7c8d"), null, new DateTime(2024, 5, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("3c4d5e6f-7a8b-49c0-e1f2-3a4b5c6d7e8f"), null, new DateTime(2024, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("3d4e5f6a-7b8c-49de-1f2a-3b4c5d6e7f8a"), null, new DateTime(2024, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 10 },
                    { new Guid("3e4f5a6b-7c8d-49e0-a1b2-3c4d5e6f7a8b"), null, new DateTime(2024, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 10 },
                    { new Guid("3e5f7c82-1abd-42f9-8e6b-0d94c3a58f27"), null, new DateTime(2024, 8, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 7 },
                    { new Guid("4b1c0d9e-8f2a-4b3c-5d6e-7f8a9b0c1d2e"), null, new DateTime(2024, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("4c5d6e7f-8a9b-400c-2d3e-4f5a6b7c8d9e"), null, new DateTime(2024, 5, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 10 },
                    { new Guid("4d5e6f7a-8b9c-40d1-f2a3-4b5c6d7e8f9a"), null, new DateTime(2024, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("4e5f6a7b-8c9d-40ef-2a3b-4c5d6e7f8a9b"), null, new DateTime(2024, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("4e9db5a7-08c1-49f2-b3e6-7d82510f6ca9"), null, new DateTime(2024, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 8 },
                    { new Guid("4f5a6b7c-8d9e-40f1-b2c3-4d5e6f7a8b9c"), null, new DateTime(2024, 3, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 8 },
                    { new Guid("5a6b7c8d-9e0f-41a2-c3d4-5e6f7a8b9c0d"), null, new DateTime(2024, 3, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("5b6c7d8e-9f0a-41bc-d3e4-5f6a7b8c9d0e"), null, new DateTime(2024, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 9 },
                    { new Guid("5c2d1e0f-9a3b-4c4d-6e7f-8a9b0c1d2e3f"), null, new DateTime(2024, 2, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 9 },
                    { new Guid("5d2e8b47-a0f3-491c-b6d5-e7940c38af26"), null, new DateTime(2024, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 7 },
                    { new Guid("5d6e7f8a-9b0c-41d2-3e4f-5a6b7c8d9e0f"), null, new DateTime(2024, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 7 },
                    { new Guid("5e6f7a8b-9c0d-41e2-a3b4-5c6d7e8f9a0b"), null, new DateTime(2024, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 7 },
                    { new Guid("5f6a7b8c-9d0e-41fa-3b4c-5d6e7f8a9b0c"), null, new DateTime(2024, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 8 },
                    { new Guid("6b7c8d9e-0f1a-42b3-d4e5-6f7a8b9c0d1e"), null, new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 10 },
                    { new Guid("6c7d8e9f-0a1b-42cd-e4f5-6a7b8c9d0e1f"), null, new DateTime(2024, 4, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 10 },
                    { new Guid("6d3e2f1a-0b4c-4d5e-7f8a-9b0c1d2e3f4a"), null, new DateTime(2024, 2, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 10 },
                    { new Guid("6d9e0f34-82a5-4b1c-b7d8-5e30c9f16a72"), null, new DateTime(2024, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 9 },
                    { new Guid("6e7f8a9b-0c1d-42e3-4f5a-6b7c8d9e0f1a"), null, new DateTime(2024, 5, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 9 },
                    { new Guid("6f7a8b9c-0d1e-42f3-b4c5-6d7e8f9a0b1c"), null, new DateTime(2024, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 10 },
                    { new Guid("7b8c9d0e-1f2a-43b4-c5d6-7e8f9a0b1c2d"), null, new DateTime(2024, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 10 },
                    { new Guid("7c8d9e0f-1a2b-43c4-e5f6-7a8b9c0d1e2f"), null, new DateTime(2024, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("7d8e9f0a-1b2c-43de-f5a6-7b8c9d0e1f2a"), null, new DateTime(2024, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("7db92e45-3af6-4c18-b507-29d1e6085fca"), null, new DateTime(2024, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("7e4f3a2b-1c5d-4e6f-8a9b-0c1d2e3f4a5b"), null, new DateTime(2024, 2, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 7 },
                    { new Guid("7f8a9b0c-1d2e-43f4-5a6b-7c8d9e0f1a2b"), null, new DateTime(2024, 5, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("87a1bf23-c64d-40e5-b9a7-15f2d8c09e4a"), null, new DateTime(2024, 8, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("8a9b0c1d-2e3f-44a5-6b7c-8d9e0f1a2b3c"), null, new DateTime(2024, 5, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 10 },
                    { new Guid("8c9d0e1f-2a3b-44c5-d6e7-8f9a0b1c2d3e"), null, new DateTime(2024, 4, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 8 },
                    { new Guid("8d9e0f1a-2b3c-44d5-f6a7-8b9c0d1e2f3a"), null, new DateTime(2024, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 8 },
                    { new Guid("8e9f0a1b-2c3d-44ef-a6b7-8c9d0e1f2a3b"), null, new DateTime(2024, 4, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("8f5a4b3c-2d6e-4f7a-9b0c-1d2e3f4a5b6c"), null, new DateTime(2024, 2, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("93a5b4c0-1d7e-46f8-b29a-5c06e874d3f2"), null, new DateTime(2024, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("9a6b5c4d-3e7f-4a8b-0c1d-2e3f4a5b6c7d"), null, new DateTime(2024, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 8 },
                    { new Guid("9b0c1d2e-3f4a-45b6-7c8d-9e0f1a2b3c4d"), null, new DateTime(2024, 5, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("9c5d8e63-7f21-48b4-a1e9-f30d762b85c1"), null, new DateTime(2024, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 8 },
                    { new Guid("9c6d5e4f-3a7b-4c8d-0e1f-2a3b4c5d6e7f"), null, new DateTime(2024, 2, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e5c83d7a-6f29-48b5-a371-2d94c8e75f12"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("9d0e1f2a-3b4c-45d6-e7f8-9a0b1c2d3e4f"), null, new DateTime(2024, 4, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("9e0f1a2b-3c4d-45e6-a7b8-9c0d1e2f3a4b"), null, new DateTime(2024, 3, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("a8c74b9e-5d12-47f3-9a6b-83c95e27d4f1"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 10 },
                    { new Guid("9f0a1b2c-3d4e-45fa-b7c8-9d0e1f2a3b4c"), null, new DateTime(2024, 4, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("e9d72f5b-a143-47c8-93b2-6e7a8c41d59f"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 10 },
                    { new Guid("a1b2c3d4-e5f6-47a8-b9c0-d1e2f3a4b5c6"), null, new DateTime(2024, 6, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("a3b2c1d0-e9f8-47a6-b5c4-d3e2f1a0b9c8"), null, new DateTime(2024, 7, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("a3b4c5d6-7e8f-49a0-1b2c-3d4e5f6a7b8c"), null, new DateTime(2024, 5, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 8 },
                    { new Guid("a3b4c5d6-7e8f-4a9b-1c2d-3e4f5a6b7c8d"), null, new DateTime(2024, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("a3b4c5d6-e7f8-49a0-b1c2-d3e4f5a6b7c8"), null, new DateTime(2024, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("a7b6c5d4-e3f2-41a0-b9c8-d7e6f5a4b3c2"), null, new DateTime(2024, 7, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("a7b8c9d0-1e2f-43a4-5b6c-7d8e9f0a1b2c"), null, new DateTime(2024, 5, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 1 },
                    { new Guid("a7b8c9d0-1e2f-4a3b-5c6d-7e8f9a0b1c2d"), null, new DateTime(2024, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 10 },
                    { new Guid("a7b8c9d0-e1f2-43a4-b5c6-d7e8f9a0b1c2"), null, new DateTime(2024, 6, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 8 },
                    { new Guid("a9b0c1d2-3e4f-4a5b-7c8d-9e0f1a2b3c4d"), null, new DateTime(2024, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("a9b8c7d6-e5f4-43a2-b1c0-d9e8f7a6b5c4"), null, new DateTime(2024, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 10 },
                    { new Guid("b0c1d2e3-4f5a-4b6c-8d9e-0f1a2b3c4d5e"), null, new DateTime(2024, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 10 },
                    { new Guid("b0c9d8e7-f6a5-44b3-c2d1-e0f9a8b7c6d5"), null, new DateTime(2024, 7, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("b2c3d4e5-6f7a-48b9-0c1d-2e3f4a5b6c7d"), null, new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("b2c3d4e5-f6a7-48b9-c0d1-e2f3a4b5c6d7"), null, new DateTime(2024, 6, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("b4c3d2e1-f0a9-48b7-c6d5-e4f3a2b1c0d9"), null, new DateTime(2024, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("b4c5d6e7-8f9a-40b1-2c3d-4e5f6a7b8c9d"), null, new DateTime(2024, 5, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 4 },
                    { new Guid("b4c5d6e7-8f9a-4b0c-2d3e-4f5a6b7c8d9e"), null, new DateTime(2024, 3, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("b4c5d6e7-f8a9-40b1-c2d3-e4f5a6b7c8d9"), null, new DateTime(2024, 6, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("b5c9a2e7-3d14-46f8-95a0-7183df462c01"), null, new DateTime(2024, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("b8c7d6e5-f4a3-42b1-c9d0-e8f7a6b5c4d3"), null, new DateTime(2024, 7, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 10 },
                    { new Guid("b8c9d0e1-2f3a-44b5-6c7d-8e9f0a1b2c3d"), null, new DateTime(2024, 5, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 9 },
                    { new Guid("b8c9d0e1-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), null, new DateTime(2024, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("b8c9d0e1-f2a3-44b5-c6d7-e8f9a0b1c2d3"), null, new DateTime(2024, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 10 },
                    { new Guid("c0e7d59a-42f1-48b3-9165-7a84f3d20b8c"), null, new DateTime(2024, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 7 },
                    { new Guid("c1d0e9f8-a7b6-45c4-d3e2-f1a0b9c8d7e6"), null, new DateTime(2024, 7, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("c1d2e3f4-5a6b-4c7d-9e0f-1a2b3c4d5e6f"), null, new DateTime(2024, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 8 },
                    { new Guid("c3d4e5f6-7a8b-49c0-1d2e-3f4a5b6c7d8e"), null, new DateTime(2024, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 2 },
                    { new Guid("c3d4e5f6-a7b8-49c0-d1e2-f3a4b5c6d7e8"), null, new DateTime(2024, 6, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("c5d4e3f2-a1b0-49c8-d7e6-f5a4b3c2d1e0"), null, new DateTime(2024, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 10 },
                    { new Guid("c5d6e7f8-9a0b-41c2-3d4e-5f6a7b8c9d0e"), null, new DateTime(2024, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 7 },
                    { new Guid("c5d6e7f8-9a0b-4c1d-3e4f-5a6b7c8d9e0f"), null, new DateTime(2024, 3, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 9 },
                    { new Guid("c5d6e7f8-a9b0-41c2-d3e4-f5a6b7c8d9e0"), null, new DateTime(2024, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("c9d0e1f2-3a4b-45c6-7d8e-9f0a1b2c3d4e"), null, new DateTime(2024, 5, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 10 },
                    { new Guid("c9d0e1f2-3a4b-4c5d-7e8f-9a0b1c2d3e4f"), null, new DateTime(2024, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 8 },
                    { new Guid("c9d0e1f2-a3b4-45c6-d7e8-f9a0b1c2d3e4"), null, new DateTime(2024, 6, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 9 },
                    { new Guid("c9d8e7f6-a5b4-43c2-d0e1-f9a8b7c6d5e4"), null, new DateTime(2024, 7, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("d0e1f2a3-4b5c-46d7-8e9f-0a1b2c3d4e5f"), null, new DateTime(2024, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 2 },
                    { new Guid("d0e1f2a3-4b5c-4d6e-8f9a-0b1c2d3e4f5a"), null, new DateTime(2024, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 7 },
                    { new Guid("d0e1f2a3-b4c5-46d7-e8f9-a0b1c2d3e4f5"), null, new DateTime(2024, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 8 },
                    { new Guid("d0e9f8a7-b6c5-44d3-e1f2-a0b9c8d7e6f5"), null, new DateTime(2024, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("d2a6b9f1-4c53-47e0-9387-61a50fc8d249"), null, new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("d4e5f6a7-8b9c-40d1-2e3f-4a5b6c7d8e9f"), null, new DateTime(2024, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("d4e5f6a7-b8c9-40d1-e2f3-a4b5c6d7e8f9"), null, new DateTime(2024, 6, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("d6e5f4a3-b2c1-40d9-e8f7-a6b5c4d3e2f1"), null, new DateTime(2024, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("d6e7f8a9-0b1c-4d2e-4f5a-6b7c8d9e0f1a"), null, new DateTime(2024, 3, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 7 },
                    { new Guid("e1f0a9b8-c7d6-45e4-f2a3-b1c0d9e8f7a6"), null, new DateTime(2024, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 9 },
                    { new Guid("e1f23c7a-9b56-48d0-a4e7-0c3d9f82b615"), null, new DateTime(2024, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("e1f2a3b4-5c6d-47e8-9f0a-1b2c3d4e5f6a"), null, new DateTime(2024, 5, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 6 },
                    { new Guid("e1f2a3b4-5c6d-4e7f-9a0b-1c2d3e4f5a6b"), null, new DateTime(2024, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 10 },
                    { new Guid("e1f2a3b4-c5d6-47e8-f9a0-b1c2d3e4f5a6"), null, new DateTime(2024, 6, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("e5f6a7b8-9c0d-41e2-3f4a-5b6c7d8e9f0a"), null, new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 4 },
                    { new Guid("e5f6a7b8-c9d0-41e2-f3a4-b5c6d7e8f9a0"), null, new DateTime(2024, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 7 },
                    { new Guid("e7f6a5b4-c3d2-41e0-f9a8-b7c6d5e4f3a2"), null, new DateTime(2024, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 8 },
                    { new Guid("e7f8a9b0-1c2d-4e3f-5a6b-7c8d9e0f1a2b"), null, new DateTime(2024, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("f18d0c37-6a95-48be-921f-5e4a7b9d0c38"), null, new DateTime(2024, 8, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 9 },
                    { new Guid("f2a1b0c9-d8e7-46f5-a3b4-c2d1e0f9a8b7"), null, new DateTime(2024, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 7 },
                    { new Guid("f2a3b4c5-6d7e-48f9-0a1b-2c3d4e5f6a7b"), null, new DateTime(2024, 5, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 3 },
                    { new Guid("f2a3b4c5-6d7e-4f8a-0b1c-2d3e4f5a6b7c"), null, new DateTime(2024, 3, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("f2a3b4c5-d6e7-48f9-a0b1-c2d3e4f5a6b7"), null, new DateTime(2024, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 7 },
                    { new Guid("f6a7b8c9-0d1e-42f3-4a5b-6c7d8e9f0a1b"), null, new DateTime(2024, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4e2b8f7-3c19-45a6-90d2-17f8e3c95b0a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 7 },
                    { new Guid("f6a7b8c9-d0e1-42f3-a4b5-c6d7e8f9a0b1"), null, new DateTime(2024, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("f8a7b6c5-d4e3-42f1-a0b9-c8d7e6f5a4b3"), null, new DateTime(2024, 7, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d9e2c5a8-7b14-4f83-9d6a-2c85e4b7f930"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("f8a9b0c1-2d3e-4f4a-6b7c-8d9e0f1a2b3c"), null, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e51d93-b24a-48f6-95c0-7d38e96f2a45"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 }
                });

            migrationBuilder.InsertData(
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ParentSolutionID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), null, "How can high-quality ideas from everyday users be surfaced without being buried by noise or popularity bias?\n\nIn collaborative platforms like Atlas, ensuring that high-quality contributions receive appropriate visibility is crucial for maintaining user engagement and facilitating problem-solving.\n\nCurrently, many platforms struggle with this challenge: valuable content can be buried while sensationalist or low-quality content rises to prominence. This undermines the collective intelligence of online communities and discourages thoughtful participation.\n\nKey questions include:\n\n- How can we design discovery mechanisms that surface valuable content without creating perverse incentives?\n\n- What balance should be struck between algorithmic and human curation?\n\n- How can we ensure that new contributors have a fair chance at visibility while still maintaining quality standards?\n\n- What metrics beyond simple engagement best indicate the actual value of contributions?\n\nThese challenges are particularly relevant for a platform like Atlas that aims to harness collective intelligence for problem-solving rather than simply maximizing engagement.", 1, new DateTime(2024, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Discoverability and Visibility of Contributions" },
                    { new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), null, "How do you ensure contributors are credited appropriately, especially if their ideas are developed or repurposed by others?\n\nIn collaborative problem-solving environments like Atlas, ideas often evolve through iterative refinement and combination with other perspectives. While this process is essential for developing robust solutions, it presents challenges for ensuring proper attribution and recognition of intellectual contributions.\n\nTraditional intellectual property frameworks are often ill-suited for collaborative platforms where the goal is shared knowledge creation rather than exclusive ownership. Yet, proper attribution remains crucial for maintaining trust, encouraging participation, and respecting contributors' work.\n\nKey questions include:\n\n- What mechanisms can track the provenance of ideas as they evolve through collaborative refinement?\n\n- How can we balance recognition of original contributors with acknowledgment of those who significantly develop or improve ideas?\n\n- What role should automated systems play in tracking contributions versus relying on community norms and practices?\n\n- How can attribution be made transparent without creating excessive overhead that impedes collaboration?\n\n- What recourse should be available when contributors feel their contributions have been misattributed or used without proper credit?\n\nSolving these challenges is essential for creating a collaborative environment where contributors feel their work is respected and valued, while still enabling the free flow of ideas necessary for collective problem-solving.", 1, new DateTime(2024, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Intellectual Property and Attribution" },
                    { new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), null, "What kind of moderation is required to keep discourse civil, inclusive, and focused—without being overly censorious?\n\nCreating an environment for productive problem-solving requires balancing freedom of expression with the need for respectful, constructive dialogue. Traditional moderation approaches often struggle with this balance, either allowing harmful behavior that drives away valuable contributors or implementing restrictions that stifle legitimate discussion.\n\nFor a platform like Atlas that aims to harness collective intelligence, this challenge is particularly critical. The governance model must support robust debate while preventing the toxicity that plagues many online spaces.\n\nKey questions include:\n\n- How can moderation systems distinguish between passionate disagreement and harmful behavior?\n\n- What role should community governance play versus centralized moderation?\n\n- How can moderation decisions be made transparent and accountable?\n\n- What escalation paths should exist when users disagree with moderation decisions?\n\n- How can the platform's design itself encourage constructive behavior and reduce the need for active moderation?\n\n- What metrics can measure the health of discourse without creating perverse incentives?\n\nDeveloping effective governance models is essential for creating an environment where diverse perspectives can contribute to solving complex problems without descending into unproductive conflict.", 1, new DateTime(2024, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Moderation and Governance of Public Debates" },
                    { new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), null, "Should users be able to post anonymously or pseudonymously? How does that affect accountability and trust?\n\nThe question of identity and attribution on collaborative platforms presents a fundamental tension between competing values. On one hand, anonymity and pseudonymity can enable participation from vulnerable populations, protect against retaliation, and allow ideas to be evaluated on their merits rather than their source. On the other hand, these practices can reduce accountability, enable harassment, and potentially undermine trust in the system.\n\nFor a platform like Atlas that aims to foster collective problem-solving, navigating this tension is particularly important. The credibility of solutions may depend on transparent expertise, while the diversity of perspectives may require protecting contributors' identities in some contexts.\n\nKey questions include:\n\n- What granular options between full identification and complete anonymity might provide appropriate balance for different contexts?\n\n- How can reputation systems function effectively when identities may be fluid or concealed?\n\n- What verification mechanisms might establish credibility without requiring full identity disclosure?\n\n- How can platforms prevent abuse of anonymity while preserving its benefits for legitimate uses?\n\n- What community norms and technical systems can establish trust in contributions despite potential identity concealment?\n\n- How might different types of content or actions require different levels of identity verification?\n\nBalancing these considerations requires thoughtful design that respects both the values of transparency and the legitimate needs for privacy and protection in online discourse.", 1, new DateTime(2024, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Balancing Transparency with Anonymity" },
                    { new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), null, "How can the think tank be accessible across languages, cultures, and digital literacy levels?\n\nFor a platform like Atlas to achieve its goal of harnessing collective intelligence for problem-solving, it must be accessible to diverse participants worldwide. However, current collaborative platforms often face significant barriers related to language, cultural context, and varying levels of digital literacy.\n\nLanguage barriers can exclude valuable perspectives, while cultural differences in communication styles and norms may lead to misunderstandings or alienation. Additionally, complex interfaces and features can exclude participants with limited digital experience or access to technology.\n\nKey questions include:\n\n- What translation and localization approaches can make content accessible while preserving nuance and context?\n\n- How can user interfaces be designed to be intuitive across cultural contexts and digital literacy levels?\n\n- What alternative access methods could accommodate participants with limited internet connectivity or devices?\n\n- How can the platform's information architecture accommodate different cultural frameworks for organizing knowledge?\n\n- What community norms and facilitation approaches can bridge cultural differences in communication styles?\n\n- How can content moderation be culturally sensitive while maintaining consistent standards?\n\n- What technical solutions might reduce bandwidth requirements for participation?\n\nAddressing these challenges is essential for building a truly global collaborative platform that can leverage diverse perspectives from around the world, rather than only those from privileged communities with high technological access and specific cultural backgrounds.", 1, new DateTime(2024, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Translation and Global Accessibility" },
                    { new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), null, "What strategies can maintain user interest and participation beyond the initial launch or viral phase?\n\nWhile many platforms experience strong initial engagement, sustaining meaningful participation over time remains a significant challenge. For Atlas to effectively leverage collective intelligence for problem-solving, it must overcome the tendency toward declining engagement that affects most collaborative platforms.\n\nTraditional social media often relies on addictive design patterns to maintain engagement, but these approaches frequently lead to shallow interaction rather than thoughtful participation. A platform focused on collaborative problem-solving requires different strategies to sustain long-term community involvement.\n\nKey questions include:\n\n- How can the platform create meaningful progression systems that reward deepening contribution without gamifying in ways that distort participation?\n\n- What feedback mechanisms best help users understand their impact and the value of their contributions?\n\n- How can community rituals and regular events create sustainable rhythms of participation?\n\n- What role should real-world impact and implementation of solutions play in maintaining motivation?\n\n- How can the platform support different modes of engagement that accommodate varying levels of time commitment and expertise?\n\n- What governance structures allow the community to evolve with changing needs while maintaining coherent purpose?\n\n- How can the platform encourage meaningful relationships between participants that strengthen commitment to the community?\n\nAddressing these challenges requires balancing intrinsic and extrinsic motivations while creating structures that support sustained, meaningful participation without burnout or disillusionment.", 1, new DateTime(2024, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Sustaining Long-Term Engagement" },
                    { new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), null, "How can the Atlas platform prevent or mitigate users who post misleading information, trolls, or coordinated disinformation efforts?\n\nTraditional social media platforms struggle with combating misinformation and bad faith participation without resorting to heavy-handed moderation that risks stifling legitimate discourse. This challenge is particularly acute for a platform like Atlas that aims to foster collaborative problem-solving.\n\nKey questions include:\n\n- What verification mechanisms can be implemented that balance accuracy with accessibility?\n\n- How can the platform's reputation system be designed to reward good-faith participation while discouraging manipulation?\n\n- What role should community moderation play versus automated systems?\n\n- How can the platform distinguish between honest mistakes and deliberate misinformation?\n\n- What safeguards can prevent coordinated manipulation campaigns while protecting privacy?\n\nAddressing these challenges is essential for maintaining the integrity of discussions and ensuring that Atlas fulfills its potential as a space for meaningful collaborative problem-solving rather than becoming another vector for misinformation.", 1, new DateTime(2024, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Misinformation and Bad Faith Participation" },
                    { new Guid("d6c9a5b4-3e2f-47d1-8c7a-5b9e6f4d3c2a"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), null, "The Southern Resident orca population—a distinct and culturally significant group of killer whales in the Pacific Northwest—is in critical decline. Once a thriving community, their numbers have dropped to dangerously low levels, with fewer than 75 individuals remaining. This decline signals more than just the loss of a species—it reflects a broader ecological crisis in the Salish Sea and surrounding marine environments.\n\nThe core issue is multifaceted. Southern Resident orcas face severe food shortages, particularly a decline in Chinook salmon—their primary prey—due to overfishing, damming of rivers, and habitat degradation. They are also threatened by increasing underwater noise from commercial vessels, which interferes with their ability to communicate and hunt. Additionally, toxic pollutants accumulate in their bodies, compromising their immune and reproductive systems over time.\n\nThis is not a natural decline—it is a direct result of human impact. Without urgent and coordinated intervention, this unique and deeply intelligent population risks extinction within our lifetime. The loss would not only be ecological but cultural, especially for Indigenous communities who view the orcas as relatives and symbols of environmental stewardship.\n\nSaving the Southern Residents requires bold action: restoring salmon habitats, reducing vessel noise, regulating pollution, and rethinking regional development. Their survival is a test of our willingness to protect vulnerable ecosystems and to act before it's too late.", 1, new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e8f2a395-c16d-48c1-b31c-d7c5a622b2f5"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Decline of Southern Resident Orca Population" },
                    { new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), null, "How can the platform ensure diverse voices are heard and prevent dominance by already-privileged demographics?\n\nCollaborative platforms often inadvertently reproduce or amplify existing societal inequalities in who participates and whose contributions receive attention. For a platform like Atlas that aims to leverage collective intelligence to solve complex problems, ensuring diverse participation is not just a matter of fairness but also essential for developing comprehensive, effective solutions.\n\nMany current platforms struggle with representation issues across dimensions like gender, race, socioeconomic status, disability, geographic location, and educational background. These disparities limit the range of perspectives and expertise available to address challenges.\n\nKey questions include:\n\n- What design features can reduce barriers to participation for underrepresented groups?\n\n- How can discovery algorithms be designed to surface valuable contributions from diverse participants rather than reinforcing existing visibility advantages?\n\n- What metrics should be tracked to identify representation gaps without creating privacy concerns?\n\n- How can the platform encourage inclusive dialogue without tokenizing contributors from underrepresented groups?\n\n- What community norms and moderation approaches can prevent behaviors that disproportionately drive away participants from marginalized groups?\n\n- How can the platform's structure acknowledge and address the different resources (time, technical access, etc.) available to different potential participants?\n\nAddressing these challenges requires thoughtful design at all levels—from technical infrastructure to community governance—to create an environment where diverse perspectives can meaningfully contribute to problem-solving.", 1, new DateTime(2024, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Bias and Representation in Participation" },
                    { new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), null, "Many individuals leaving foster care, prison, or military service face significant challenges in transitioning to stable, independent living. Gaps in transitional services often leave these populations without the support needed to secure housing, employment, healthcare, and social connections, increasing their risk of homelessness and long-term instability.\n\nFor youth aging out of foster care, the abrupt end of support can mean navigating adulthood without family, financial resources, or guidance. Formerly incarcerated individuals encounter barriers to employment, housing, and social reintegration, often compounded by stigma and legal restrictions. Veterans and those leaving military service may struggle with mental health issues, physical injuries, and the challenge of adapting to civilian life.\n\nTransitional programs are frequently underfunded, fragmented, or difficult to access. Eligibility requirements, waitlists, and bureaucratic hurdles can prevent those most in need from receiving timely help. Coordination between agencies is often lacking, resulting in missed opportunities for early intervention and support.\n\nAddressing these gaps requires comprehensive, well-resourced transitional services that prioritize prevention, empowerment, and long-term stability. Solutions include expanding case management, peer support, housing assistance, job training, and mental health care tailored to the unique needs of each population. By strengthening transitional services, we can reduce the risk of homelessness and promote successful reintegration into society.", 1, new DateTime(2024, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("6fa94c21-d827-4b0e-a3f5-7e9b8d512c39"), null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Gaps in Transitional Services After Foster Care, Prison, or Military Service" }
                });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("01278901-3cd4-e5f6-789a-bcdef0123456"), null, new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 10 },
                    { new Guid("0e7f8a9b-6c5d-4e3f-1a2b-9c8d7e6f5a4b"), null, new DateTime(2024, 3, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 4 },
                    { new Guid("0f9a8b7c-6d5e-4f3a-2b1c-9d8c7b6a5f4e"), null, new DateTime(2024, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("12389012-4de5-f607-89ab-cdef01234567"), null, new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee46"), 11 },
                    { new Guid("1a0b9c8d-7e6f-4a4b-3c2d-0e9d8c7b6a5f"), null, new DateTime(2024, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 8 },
                    { new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), null, new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("1f8a9b0c-7d6e-5f4a-2b3c-0d9e8f7a6b5c"), null, new DateTime(2024, 3, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 1 },
                    { new Guid("23490123-5ef6-0789-abcd-ef012345678a"), null, new DateTime(2024, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee47"), 12 },
                    { new Guid("2a9b0c1d-8e7f-6a5b-3c4d-1e0f9a8b7c6d"), null, new DateTime(2024, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("2b1c0d9e-8f7a-4b5c-4d3e-1f0e9d8c7b6a"), null, new DateTime(2024, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 9 },
                    { new Guid("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), null, new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("2f8a7d5e-4b9c-48e1-a036-7c53f4e8d12b"), null, new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e7d9c6b2-3a5f-4182-9e08-51bd72af46c3"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 6 },
                    { new Guid("34501234-6f07-89ab-cdef-0123456789ab"), null, new DateTime(2024, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee48"), 12 },
                    { new Guid("3b0c1d2e-9f8a-7b6c-4d5e-2f1a0b9c8d7e"), null, new DateTime(2024, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 2 },
                    { new Guid("3c2d1e0f-9a8b-4c6d-5e4f-2a1f0e9d8c7b"), null, new DateTime(2024, 9, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), null, new DateTime(2024, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 10 },
                    { new Guid("45612345-7a89-bcde-f012-3456789abcde"), null, new DateTime(2024, 1, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee49"), 13 },
                    { new Guid("4c1d2e3f-0a9b-8c7d-5e6f-3a2b1c0d9e8f"), null, new DateTime(2024, 3, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 3 },
                    { new Guid("4d3e2f1a-0b9c-4d7e-6f5a-3b2a1f0e9d8c"), null, new DateTime(2024, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 8 },
                    { new Guid("4d5e6f7a-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), null, new DateTime(2024, 1, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 10 },
                    { new Guid("5a4b3c2d-1e0f-4a8b-9c7d-6e5f4d3c2b1a"), null, new DateTime(2024, 9, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("5d7c8f3a-2e6b-4c1d-9a3f-0e8d7b5c6a4f"), null, new DateTime(2024, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 2 },
                    { new Guid("5e4f3a2b-1c0d-4e8f-7a6b-4c3b2a1f0e9d"), null, new DateTime(2024, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 9 },
                    { new Guid("5e6f7a8b-9c0d-1e2f-3a4b-5c6d7e8f9a0b"), null, new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 10 },
                    { new Guid("6a3b5c8d-1e7f-4a9b-8c5d-2e4f6a7b8c9d"), null, new DateTime(2024, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 1 },
                    { new Guid("6b5c4d3e-2f1a-4b9c-8d7e-5f4e3d2c1b0a"), null, new DateTime(2024, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("6f5a4b3c-2d1e-4f9a-8b7c-5d4c3b2a1f0e"), null, new DateTime(2024, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("6f7a8b9c-0d1e-2f3a-4b5c-6d7e8f9a0b1c"), null, new DateTime(2024, 1, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("7a6b5c4d-3e2f-4a0b-9c8d-6e5d4c3b2a1f"), null, new DateTime(2024, 9, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 7 },
                    { new Guid("7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d"), null, new DateTime(2024, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 10 },
                    { new Guid("7b4c6d5e-3f2a-1b9c-8d7e-6f5a4b3c2d1e"), null, new DateTime(2024, 3, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 3 },
                    { new Guid("7c6d5e4f-3a2b-4c0d-9e8f-6a5f4e3d2c1b"), null, new DateTime(2024, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 9 },
                    { new Guid("8b7c6d5e-4f3a-4b1c-0d9e-7f6e5d4c3b2a"), null, new DateTime(2024, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("8b9c0d1e-2f3a-4b5c-6d7e-8f9a0b1c2d3e"), null, new DateTime(2024, 1, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 10 },
                    { new Guid("8c5d6e7f-4a3b-2c1d-9e8f-7a6b5c4d3e2f"), null, new DateTime(2024, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 5 },
                    { new Guid("8d7e6f5a-4b3c-4d1e-0f9a-7b6a5f4e3d2c"), null, new DateTime(2024, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 7 },
                    { new Guid("96b5c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7c"), null, new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 7 },
                    { new Guid("9c0d1e2f-3a4b-5c6d-7e8f-9a0b1c2d3e4f"), null, new DateTime(2024, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 10 },
                    { new Guid("9c8d7e6f-5a4b-4c2d-1e0f-8a7f6e5d4c3b"), null, new DateTime(2024, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("9d6e7f8a-5b4c-3d2e-0f1a-8b7c6d5e4f3a"), null, new DateTime(2024, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b4e7f8d2-9c3a-45b1-87d6-0e9f2c5a4b3d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 2 },
                    { new Guid("9e8f7a6b-5c4d-4e2f-1a0b-8c7b6a5f4e3d"), null, new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"), null, new DateTime(2024, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 6 },
                    { new Guid("a1b2c3d4-e5f6-4a7b-8c9d-1e2f3a4b5c6d"), null, new DateTime(2024, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 7 },
                    { new Guid("a1b7f6e5-8d32-47c0-b965-2f48a3b1c7d6"), null, new DateTime(2024, 10, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 10 },
                    { new Guid("a1f0e9d8-7c64-41b5-b309-4e82f7a5b1c0"), null, new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("a3f2e1d0-9c86-43b7-b521-6e04f9a7b3c2"), null, new DateTime(2024, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("a5b1f0e9-2d76-41c4-b309-6f82a7b5c1d0"), null, new DateTime(2024, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 9 },
                    { new Guid("a6b5c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7b"), null, new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("a7b3f2e1-4d98-43c6-b521-8f04a9b7c3d2"), null, new DateTime(2024, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 10 },
                    { new Guid("a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d"), null, new DateTime(2024, 4, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 4 },
                    { new Guid("a7b8c9d0-e1f2-4a3b-5c6d-7e8f9a0b1c2d"), null, new DateTime(2024, 4, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 3 },
                    { new Guid("a7f6e5d4-3c20-47b1-b965-0e48f3a1b7c6"), null, new DateTime(2024, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 9 },
                    { new Guid("abc12345-de67-89f0-1234-56789abcdef0"), null, new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 9 },
                    { new Guid("b2a1f0e9-8d75-42c6-b410-5f93a8b6c2d1"), null, new DateTime(2024, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"), null, new DateTime(2024, 4, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 6 },
                    { new Guid("b2c3d4e5-f6a7-4b8c-9d0e-2f3a4b5c6d7e"), null, new DateTime(2024, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 5 },
                    { new Guid("b2c8a7f6-9e43-48d1-b076-3a59b4c2d8e7"), null, new DateTime(2024, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("b4a3f2e1-0d97-44c8-b632-7f15a0b8c4d3"), null, new DateTime(2024, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 10 },
                    { new Guid("b5a6c4f1-e2d3-47a8-9b0c-1d2e3f4a5b7a"), null, new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 8 },
                    { new Guid("b6c2a1f0-3e87-42d5-b410-7a93b8c6d2e1"), null, new DateTime(2024, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 10 },
                    { new Guid("b8a7f6e5-4d31-48c2-b076-1f59a4b2c8d7"), null, new DateTime(2024, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 8 },
                    { new Guid("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e"), null, new DateTime(2024, 4, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 9 },
                    { new Guid("b8c9d0e1-f2a3-4b5c-6d7e-8f9a0b1c2d3e"), null, new DateTime(2024, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("bcd23456-ef78-90a1-2345-6789abcdef01"), null, new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("c3b2a1f0-9e86-43d7-b521-6a04b9c7d3e2"), null, new DateTime(2024, 10, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f"), null, new DateTime(2024, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 4 },
                    { new Guid("c3d4e5f6-a7b8-4c9d-0e1f-3a4b5c6d7e8f"), null, new DateTime(2024, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("c3d9b8a7-0f54-49e2-b187-4b60c5d3e9f8"), null, new DateTime(2024, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 8 },
                    { new Guid("c4f1e2d3-a5b6-47a8-9b0c-1d2e3f4a5b6f"), null, new DateTime(2024, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 6 },
                    { new Guid("c7d3b2a1-4f98-43e6-a521-8b04c9d7e3f2"), null, new DateTime(2024, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 9 },
                    { new Guid("c9b8a7f6-5e42-49d3-b187-2a60b5c3d9e8"), null, new DateTime(2024, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f"), null, new DateTime(2024, 4, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 8 },
                    { new Guid("cde34567-f890-a1b2-3456-789abcdef012"), null, new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 10 },
                    { new Guid("d0c9b8a7-6f53-40e4-b298-3b71c6d4e0f9"), null, new DateTime(2024, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("d0e1f2a3-b4c5-4d6e-7f8a-9b0c1d2e3f4a"), null, new DateTime(2024, 4, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 4 },
                    { new Guid("d3c4f1e2-b6a5-47a8-9b0c-1d2e3f4a5b6e"), null, new DateTime(2024, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("d4c3b2a1-0f97-44e8-a632-7b15c0d8e4f3"), null, new DateTime(2024, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("d4e0c9b8-1a65-40f3-b298-5c71d6e4f0a9"), null, new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a"), null, new DateTime(2024, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 3 },
                    { new Guid("d4e5f6a7-b8c9-4d0e-1f2a-4b5c6d7e8f9a"), null, new DateTime(2024, 4, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 4 },
                    { new Guid("d6e7f8a9-b0c1-47d2-93e4-5f8c7b6a9d0e"), null, new DateTime(2024, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d2e3f2-b1a0-47c9-8d67-5f2e3b4a1d90"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 8 },
                    { new Guid("d8e4c3b2-5a09-44f7-b632-9c15d0e8f4a3"), null, new DateTime(2024, 10, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 8 },
                    { new Guid("def45678-90a1-b2c3-4567-89abcdef0123"), null, new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("e1d0c9b8-7a64-41f5-b309-4c82d7e5f1a0"), null, new DateTime(2024, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("e1f2a3b4-c5d6-4e7f-8a9b-0c1d2e3f4a5b"), null, new DateTime(2024, 4, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 7 },
                    { new Guid("e2d3c4f1-a5b6-47a8-9b0c-1d2e3f4a5b6d"), null, new DateTime(2024, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 7 },
                    { new Guid("e5d4c3b2-1a08-45f9-b743-8c26d1e9f5a4"), null, new DateTime(2024, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("e5f1d0c9-2b76-41a4-b309-6d82e7f5a1b0"), null, new DateTime(2024, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 8 },
                    { new Guid("e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b"), null, new DateTime(2024, 4, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 7 },
                    { new Guid("e5f6a7b8-c9d0-4e1f-2a3b-5c6d7e8f9a0b"), null, new DateTime(2024, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("e9f5d4c3-6b10-45a8-b743-0d26e1f9a5b4"), null, new DateTime(2024, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("ef056789-1ab2-c3d4-5678-9abcdef01234"), null, new DateTime(2024, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("f0167890-2bc3-d4e5-6789-abcdef012345"), null, new DateTime(2024, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee45"), 9 },
                    { new Guid("f0a6e5d4-7c21-46b9-b854-1e37f2a0b6c5"), null, new DateTime(2024, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("f0e9d8c7-6b53-40a4-b298-3d71e6f4a0b9"), null, new DateTime(2024, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 10 },
                    { new Guid("f1e2d3c4-b5a6-47a8-9b0c-1d2e3f4a5b6c"), null, new DateTime(2024, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a8e3d7c5-6b9f-47a2-8c51-4f9e0d25b38a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("f2a3b4c5-d6e7-4f8a-9b0c-1d2e3f4a5b6c"), null, new DateTime(2024, 4, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 6 },
                    { new Guid("f2e1d0c9-8b75-42a6-b410-5d93e8f6a2b1"), null, new DateTime(2024, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 8 },
                    { new Guid("f4a0e9d8-1c65-40b3-b298-5e71f6a4b0c9"), null, new DateTime(2024, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("f6a2e1d0-3c87-42b5-b410-7e93f8a6b2c1"), null, new DateTime(2024, 10, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c"), null, new DateTime(2024, 4, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7f6e5c4-3b2a-40d9-8e71-5f94a3c2b1d0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 5 },
                    { new Guid("f6a7b8c9-d0e1-4f2a-3b4c-6d7e8f9a0b1c"), null, new DateTime(2024, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d9a7b3-6e42-48f1-95ac-2e87d3b9c61f"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 6 },
                    { new Guid("f6e5d4c3-2b19-46a0-b854-9d37e2f0a6b5"), null, new DateTime(2024, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 }
                });

            migrationBuilder.InsertData(
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), null, "Public Education Campaigns to Reduce Stigma represents a strategic approach to changing societal perceptions and attitudes about homelessness through coordinated, evidence-based messaging and community engagement. By tackling misconceptions and humanizing the experience of housing instability, these campaigns can help dismantle one of the most significant barriers preventing people from seeking assistance.\n\nEffective stigma reduction campaigns are multi-faceted, employing various communication channels and approaches. Mass media components utilize billboards, public service announcements, social media campaigns, and traditional advertising to challenge stereotypes and present accurate information about the causes of homelessness, emphasizing structural factors like housing affordability, economic instability, and insufficient support systems rather than personal failings. These campaigns feature authentic stories and images that highlight the diversity of people experiencing homelessness, avoiding sensationalism while preserving dignity.\n\nCommunity engagement initiatives complement mass media efforts through in-person educational workshops, speaking engagements at schools and community organizations, interactive exhibits, and public forums where housed and unhoused community members can engage in facilitated dialogue. These face-to-face interactions help build empathy by creating spaces for genuine connection and understanding.\n\nPeer ambassador programs represent a particularly powerful component, training and employing individuals with lived experience of homelessness to serve as public speakers, media spokespeople, and community educators. This approach not only provides authentic representation but also creates meaningful employment opportunities and recognition of expertise gained through experience.\n\nTargeted professional education reaches service providers, healthcare workers, law enforcement, educators, and other professionals who regularly interact with people experiencing homelessness. This specialized training addresses unconscious bias, promotes trauma-informed approaches, and provides practical strategies for creating more welcoming and dignified service environments.\n\nWhen implemented comprehensively and sustained over time, public education campaigns contribute to measurable shifts in public attitudes, increased support for evidence-based solutions to homelessness, reduced discrimination in service settings, and—most importantly—greater willingness among people experiencing homelessness to seek and engage with available support services.", 1, new DateTime(2024, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("f3d6b8a9-2c41-47e5-8a93-5d7c1e9f4a8b"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Public Education Campaigns to Reduce Stigma" },
                    { new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), null, "Social media platforms should empower users with direct control over the algorithms that determine what content they see, specifically designed to mitigate political polarization and exposure to extremist content. This solution puts decision-making power back in users' hands rather than defaulting to engagement-maximizing algorithms that often amplify divisive content.\n\nThe key feature would be a transparent, user-friendly control panel offering adjustable settings including:\n\n- Political diversity sliders: Users could set preferences for seeing content across the political spectrum rather than only views that align with their existing positions\n\n- Content variety controls: Options to balance news sources, opinion pieces, and user discussions from different perspectives\n\n- Fact-checking intensity: Adjustable settings for how prominently fact-checking information appears alongside political content\n\n- Source credibility thresholds: Ability to set minimum credibility standards for news sources in one's feed\n\n- Tone preferences: Options to prioritize measured, substantive political discussions over inflammatory rhetoric\n\n- Contextual depth settings: Controls for showing more in-depth background on complex political issues rather than simplified, polarizing summaries\n\nThese controls would be accompanied by periodic feedback showing users metrics about their content diet, such as political diversity scores, emotional tone analysis, and source variety statistics. Optional recommendations could suggest small adjustments to experience more balanced political discourse.\n\nImplementation would include educational onboarding to help users understand how their choices affect their information ecosystem, default settings designed for balanced exposure, and continuous refinement based on research about what settings most effectively reduce polarization while maintaining user satisfaction.\n\nBy transferring algorithm control from platform to user, this solution directly addresses the systemic incentives that currently reward divisive content. It preserves free expression while creating pathways for users to intentionally construct healthier information environments that promote understanding across political divides rather than deepening them.", 1, new DateTime(2024, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b5a7d93c-4e28-46f1-87b3-9c5a2d41e6f8"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Make Algorithms User-Adjustable" }
                });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssueVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "IssueID", "ModifiedAt", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("19c84b3e-6f5a-47d0-a2c1-9e87f0d3b542"), null, new DateTime(2024, 8, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("2d3e4f5a-6b7c-4809-a1b2-c3d4e5f6a7b8"), null, new DateTime(2024, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 7 },
                    { new Guid("2d639f18-75ab-4e20-9c84-f06d3b1c7a5e"), null, new DateTime(2024, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("3c4d5e6f-7a8b-4921-83c0-f5e4d3c2b1a0"), null, new DateTime(2024, 8, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("3e9f7d12-0a5b-46c8-9d3e-f2a1b8c5d674"), null, new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("3f2a96e0-d5c1-47b8-8ef3-4b7d98ca61a5"), null, new DateTime(2024, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 7 },
                    { new Guid("42c83a9d-f150-4e8b-a7d2-9ef683b5c4f1"), null, new DateTime(2024, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 9 },
                    { new Guid("5d21e7f9-80b3-46a2-9c4d-1f8e0a7b6954"), null, new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 9 },
                    { new Guid("71d8e054-9c3b-48a2-bf67-30e195d84a2c"), null, new DateTime(2024, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 8 },
                    { new Guid("76b91d2a-4e5f-48c0-97d3-a8b6c5f2e70d"), null, new DateTime(2024, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 7 },
                    { new Guid("87ef24d1-9c56-48a7-b3f0-5e72d18c94ba"), null, new DateTime(2024, 8, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 8 },
                    { new Guid("91c4e7d2-5a8f-49b3-0e6d-7f8a2b1c9e05"), null, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("9a8b7c6d-5f4e-4312-b0a9-1c2d3e4f5a6b"), null, new DateTime(2024, 8, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 8 },
                    { new Guid("a0b1c2d3-e4f5-6a7b-8c9d-0e1f2a3b4c5d"), null, new DateTime(2024, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("a0f9e8d7-c6b5-4423-a132-f9e8d7c6b5a0"), null, new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 10 },
                    { new Guid("a3b4c5d6-e7f8-9a0b-1c2d-3e4f5a6b7c8d"), null, new DateTime(2024, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 10 },
                    { new Guid("a4b5c6d7-e8f9-0a1b-2c3d-4e5f6a7b8c9d"), null, new DateTime(2024, 7, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 10 },
                    { new Guid("a4b7c9e1-2d3f-4a5b-6c7d-8e9f0a1b2c3d"), null, new DateTime(2024, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("a4f3e2d1-c0b9-4685-c4d3-f1a0e9b8c7d6"), null, new DateTime(2024, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 10 },
                    { new Guid("a5b6c7d8-e9f0-1a2b-3c4d-5e6f7a8b9c0d"), null, new DateTime(2024, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 9 },
                    { new Guid("a63d9f28-7b4e-45c1-8f0a-e2d6b957c31f"), null, new DateTime(2024, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 8 },
                    { new Guid("a6f5e4d3-c2b1-4089-a798-f5e4d3c2b1a6"), null, new DateTime(2024, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee45"), 8 },
                    { new Guid("a9b8c7d6-e5f4-4312-b1a0-9d8c7b6a5f4e"), null, new DateTime(2024, 8, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 8 },
                    { new Guid("b1a0f9e8-d7c6-4534-b243-a0f9e8d7c6b1"), null, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("b1c2d3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6e"), null, new DateTime(2024, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("b4c5d6e7-f8a9-0b1c-2d3e-4f5a6b7c8d9e"), null, new DateTime(2024, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("b5a4f3e2-d1c0-4796-d5e4-a2b1f0c9d8e7"), null, new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("b5c6d7e8-f9a0-1b2c-3d4e-5f6a7b8c9d0e"), null, new DateTime(2024, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 8 },
                    { new Guid("b5c8d0e2-3f4a-5b6c-7d8e-9f0a1b2c3d4e"), null, new DateTime(2024, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 8 },
                    { new Guid("b6c7d8e9-f0a1-2b3c-4d5e-6f7a8b9c0d1e"), null, new DateTime(2024, 8, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 8 },
                    { new Guid("c1d2e3f4-a5b6-7c8d-9e0f-1a2b3c4d5e6f"), null, new DateTime(2024, 8, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("c2b1a0f9-e8d7-4645-c354-b1a0f9e8d7c2"), null, new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("c2d3e4f5-a6b7-8c9d-0e1f-2a3b4c5d6e7f"), null, new DateTime(2024, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("c5d6e7f8-a9b0-1c2d-3e4f-5a6b7c8d9e0f"), null, new DateTime(2024, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("c6b5a4f3-e2d1-4807-e6f5-b3c2a1d0e9f8"), null, new DateTime(2024, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 6 },
                    { new Guid("c6d7e8f9-a0b1-2c3d-4e5f-6a7b8c9d0e1f"), null, new DateTime(2024, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 9 },
                    { new Guid("c6d9e0f3-4a5b-6c7d-8e9f-0a1b2c3d4e5f"), null, new DateTime(2024, 6, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("5e9b3c7d-2a8f-4e16-9d7c-3a1b5e8f9d2e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 7 },
                    { new Guid("c7b8a9d0-5e2f-4983-a1b0-c9d8e7f6a5b4"), null, new DateTime(2024, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 7 },
                    { new Guid("c7d8e9f0-a1b2-3c4d-5e6f-7a8b9c0d1e2f"), null, new DateTime(2024, 8, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("c8f49e27-35a1-4d60-b8e7-2f17d0a59c36"), null, new DateTime(2024, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("d1c0b9a8-7f6e-4352-91a0-c8d7b6a5f4e3"), null, new DateTime(2024, 8, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 8 },
                    { new Guid("d1e2f3a4-b5c6-7d8e-9f0a-1b2c3d4e5f6a"), null, new DateTime(2024, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 8 },
                    { new Guid("d2e3f4a5-b6c7-8d9e-0f1a-2b3c4d5e6f7a"), null, new DateTime(2024, 8, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("d3c2b1a0-f9e8-4756-d465-c2b1a0f9e8d3"), null, new DateTime(2024, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("d3e4f5a6-b7c8-9d0e-1f2a-3b4c5d6e7f8a"), null, new DateTime(2024, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("d7c6b5a4-f3e2-4918-f7a6-c4d3b2e1f0a9"), null, new DateTime(2024, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("e1f2a3b4-c5d6-7e8f-9a0b-1c2d3e4f5a6b"), null, new DateTime(2024, 7, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("e2d1c0b9-8a7f-4463-a2b1-d9e8c7f6a5b4"), null, new DateTime(2024, 8, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("e2f3a4b5-c6d7-8e9f-0a1b-2c3d4e5f6a7b"), null, new DateTime(2024, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("e3f4a5b6-c7d8-9e0f-1a2b-3c4d5e6f7a8b"), null, new DateTime(2024, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 10 },
                    { new Guid("e4d3c2b1-a0f9-4867-e576-d3c2b1a0f9e4"), null, new DateTime(2024, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 10 },
                    { new Guid("e4f5a6b7-c8d9-0e1f-2a3b-4c5d6e7f8a9b"), null, new DateTime(2024, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("e5d09b37-c28f-46a4-95f0-1d7ce8a2b950"), null, new DateTime(2024, 8, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("e5f4d3c2-b1a0-4675-8392-c1d0b9a8f7e6"), null, new DateTime(2024, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 10 },
                    { new Guid("e8d7c6b5-a4f3-4029-a8b7-d5e4c3f2a1b0"), null, new DateTime(2024, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 7 },
                    { new Guid("e8f9a0b1-c2d3-4e5f-6a7b-8c9d0e1f2a3b"), null, new DateTime(2024, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 10 },
                    { new Guid("f1e2d3c4-b5a6-4798-87b9-d0c1e2f3a4b5"), null, new DateTime(2024, 8, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c9d8e7f6-a5b4-43c2-91d0-e8f7a6b5c4d3"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("f2e3d4c5-b6a7-8f9e-0a1b-2c3d4e5f6a7b"), null, new DateTime(2024, 7, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d4c7e8a2-5b9f-47d1-8e3a-6f2c9d0b5a4e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("f3a4b5c6-d7e8-9f0a-1b2c-3d4e5f6a7b8c"), null, new DateTime(2024, 7, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("7a9c2e4b-6d5f-48c3-9e7a-1b2d3f4c5e6a"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 7 },
                    { new Guid("f3e2d1c0-9b8a-4574-b3c2-e0f9d8a7b6c5"), null, new DateTime(2024, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b5a4c3d2-e1f0-47a9-b830-c5d4e3f2a1b0"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 7 },
                    { new Guid("f4a5b6c7-d8e9-0f1a-2b3c-4d5e6f7a8b9c"), null, new DateTime(2024, 8, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("b2e3c4d5-a6b7-48c9-9d0e-1f2a3b4c5d6e"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 7 },
                    { new Guid("f5a37e84-20d9-48b1-936c-7a0be5d12c48"), null, new DateTime(2024, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("f5a6b7c8-d9e0-1f2a-3b4c-5d6e7f8a9b0c"), null, new DateTime(2024, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("f5e4d3c2-b1a0-4978-f687-e4d3c2b1a0f5"), null, new DateTime(2024, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("f9a0b1c2-d3e4-5f6a-7b8c-9d0e1f2a3b4c"), null, new DateTime(2024, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("d7e8f9a0-b1c2-43d4-95e6-f7a8b9c0d1e2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 9 },
                    { new Guid("f9e8d7c6-b5a4-4312-9021-e8d7c6b5a4f3"), null, new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c7e8f9d0-a1b2-43c4-95d6-e7f8a9b0c1d2"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("fb249a5e-d07c-43e1-b8a5-36f1c094d782"), null, new DateTime(2024, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("f7e6d5c4-b3a2-41f0-8c9d-6e5f4a3b2c1d"), null, new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 7 }
                });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("a0b1c2d9-8e5f-40a6-c4d1-b7f0e8a5d3c2"), null, new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("a3d5c4b2-0e9f-43a8-b7b6-2c1d0e9f8a7b"), null, new DateTime(2024, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("a6b7c8d5-4e1f-46a2-c0d7-b3f6e4a1d9c8"), null, new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 7 },
                    { new Guid("a9d1c0b8-6e5f-49a4-b3b2-8c7d6e5f4a3b"), null, new DateTime(2024, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("b0e2d1c9-7f6a-40b5-b4c3-9d8e7f6a5b4c"), null, new DateTime(2024, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("b1c2d3e0-9f6a-41b7-d5e2-c8a1f9b6e4d3"), null, new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 7 },
                    { new Guid("b4e6d5c3-1f0a-44b9-a8c7-3d2e1f0a9b8c"), null, new DateTime(2024, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("b7c8d9e6-5f2a-47b3-d1e8-c4a7f5b2e0d9"), null, new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 8 },
                    { new Guid("b8e0d9c7-5f4a-48b3-a2c1-7d6e5f4a3b2c"), null, new DateTime(2024, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("c1f3e2d0-8a7b-41c6-b5d4-0e9f8a7b6c5d"), null, new DateTime(2024, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("c2d3e4f1-0a7b-42c8-e6f3-d9b2a0c7f5e4"), null, new DateTime(2024, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("c5f7e6d4-2a1b-45c0-b9d8-4e3f2a1b0c9d"), null, new DateTime(2024, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 9 },
                    { new Guid("c8d9e0f7-6a3b-48c4-e2f9-d5b8a6c3f1e0"), null, new DateTime(2024, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("c9f1e0d8-6a5b-49c4-b3d2-8e7f6a5b4c3d"), null, new DateTime(2024, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("d0a2f1e9-7b6c-40d5-b4e3-9f8a7b6c5d4e"), null, new DateTime(2024, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("d2a4f3e1-9b8c-42d7-b6e5-1f0a9b8c7d6e"), null, new DateTime(2024, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("d3e4f5a2-1b8c-43d9-f7a4-e0c3b1d8a6f5"), null, new DateTime(2024, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("d6a8f7e5-3b2c-46d1-b0e9-5f4a3b2c1d0e"), null, new DateTime(2024, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 8 },
                    { new Guid("d9e0f1a8-7b4c-49d5-f3a0-e6c9b7d4a2f1"), null, new DateTime(2024, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 10 },
                    { new Guid("e1b3a2f0-8c7d-41e6-b5f4-0a9b8c7d6e5f"), null, new DateTime(2024, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("e4f5a6b3-2c9d-44e0-a8b5-f1d4c2e9b7a6"), null, new DateTime(2024, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("e7b9a8f6-4c3d-47e2-b1f0-6a5b4c3d2e1f"), null, new DateTime(2024, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("e8f9a0b7-6c3d-48e4-a2b9-f5d8c6e3a1b0"), null, new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("f2c4b3a1-9d8e-42f7-b6a5-1b0c9d8e7f6a"), null, new DateTime(2024, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("f5a6b7c4-3d0e-45f1-b9c6-a2e5d3f0c8b7"), null, new DateTime(2024, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 9 },
                    { new Guid("f8c0b9a7-5d4e-48f3-b2a1-7b6c5d4e3f2a"), null, new DateTime(2024, 10, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("f9a0b1c8-7d4e-49f5-b3c0-a6e9d7f4b2c1"), null, new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 10 }
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
                name: "IX_CommentVotes_CommentID",
                schema: "comments",
                table: "CommentVotes",
                column: "CommentID");

            migrationBuilder.CreateIndex(
                name: "IX_CommentVotes_UserID",
                schema: "comments",
                table: "CommentVotes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_AuthorID",
                table: "Issues",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_BlockedContentID",
                table: "Issues",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ParentIssueID",
                table: "Issues",
                column: "ParentIssueID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ParentSolutionID",
                table: "Issues",
                column: "ParentSolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ScopeID",
                table: "Issues",
                column: "ScopeID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuesCategories_IssueID",
                schema: "issues",
                table: "IssuesCategories",
                column: "IssueID");

            migrationBuilder.CreateIndex(
                name: "IX_IssueVotes_AppUserId",
                schema: "issues",
                table: "IssueVotes",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueVotes_IssueID",
                schema: "issues",
                table: "IssueVotes",
                column: "IssueID");

            migrationBuilder.CreateIndex(
                name: "IX_IssueVotes_UserID",
                schema: "issues",
                table: "IssueVotes",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_AuthorID",
                table: "Solutions",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_BlockedContentID",
                table: "Solutions",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_ParentIssueID",
                table: "Solutions",
                column: "ParentIssueID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_ScopeID",
                table: "Solutions",
                column: "ScopeID");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionsCategories_SolutionID",
                schema: "solutions",
                table: "SolutionsCategories",
                column: "SolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionVotes_AppUserId",
                schema: "solutions",
                table: "SolutionVotes",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SolutionVotes_SolutionID",
                schema: "solutions",
                table: "SolutionVotes",
                column: "SolutionID");

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
                principalTable: "Issues",
                principalColumn: "IssueID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Solutions_SolutionID",
                schema: "comments",
                table: "Comments",
                column: "SolutionID",
                principalTable: "Solutions",
                principalColumn: "SolutionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Solutions_ParentSolutionID",
                table: "Issues",
                column: "ParentSolutionID",
                principalTable: "Solutions",
                principalColumn: "SolutionID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_AspNetUsers_AuthorID",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_AspNetUsers_AuthorID",
                table: "Solutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Issues_BlockedContent_BlockedContentID",
                table: "Issues");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_BlockedContent_BlockedContentID",
                table: "Solutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Solutions_Issues_ParentIssueID",
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
                name: "IssuesCategories",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "IssueVotes",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "SolutionsCategories",
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
                name: "Categories",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Comments",
                schema: "comments");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BlockedContent",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "Solutions");

            migrationBuilder.DropTable(
                name: "Scopes",
                schema: "app");
        }
    }
}
