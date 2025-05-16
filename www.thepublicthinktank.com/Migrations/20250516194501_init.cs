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
                name: "issues");

            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                schema: "issues",
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
                schema: "issues",
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
                schema: "issues",
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
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                name: "Issues",
                schema: "issues",
                columns: table => new
                {
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScopeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentIssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BlockedContentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                        principalSchema: "issues",
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
                        principalSchema: "issues",
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
                        principalSchema: "issues",
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssuesCategories_Issues_IssueID",
                        column: x => x.IssueID,
                        principalSchema: "issues",
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solutions",
                schema: "issues",
                columns: table => new
                {
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    AuthorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlockedContentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                        principalSchema: "issues",
                        principalTable: "BlockedContent",
                        principalColumn: "BlockedContentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solutions_Issues_IssueID",
                        column: x => x.IssueID,
                        principalSchema: "issues",
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                schema: "issues",
                columns: table => new
                {
                    CommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IssueSolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    ContentStatus = table.Column<int>(type: "int", nullable: false),
                    AuthorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentCommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                        principalSchema: "issues",
                        principalTable: "BlockedContent",
                        principalColumn: "BlockedContentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentID",
                        column: x => x.ParentCommentID,
                        principalSchema: "issues",
                        principalTable: "Comments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Issues_IssueID",
                        column: x => x.IssueID,
                        principalSchema: "issues",
                        principalTable: "Issues",
                        principalColumn: "IssueID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Solutions_IssueSolutionID",
                        column: x => x.IssueSolutionID,
                        principalSchema: "issues",
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
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IssueSolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                        principalSchema: "issues",
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
                        name: "FK_UserHistory_Solutions_IssueSolutionID",
                        column: x => x.IssueSolutionID,
                        principalSchema: "issues",
                        principalTable: "Solutions",
                        principalColumn: "SolutionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserVotes",
                schema: "issues",
                columns: table => new
                {
                    VoteID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VoteValue = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssueID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IssueSolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    SolutionID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserCommentCommentID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVotes", x => x.VoteID);
                    table.ForeignKey(
                        name: "FK_UserVotes_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVotes_Comments_UserCommentCommentID",
                        column: x => x.UserCommentCommentID,
                        principalSchema: "issues",
                        principalTable: "Comments",
                        principalColumn: "CommentID");
                    table.ForeignKey(
                        name: "FK_UserVotes_Issues_IssueID",
                        column: x => x.IssueID,
                        principalSchema: "issues",
                        principalTable: "Issues",
                        principalColumn: "IssueID");
                    table.ForeignKey(
                        name: "FK_UserVotes_Solutions_SolutionID",
                        column: x => x.SolutionID,
                        principalSchema: "issues",
                        principalTable: "Solutions",
                        principalColumn: "SolutionID");
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "1a61454c-5b83-4aab-8661-96d6dffbe31", 0, "a1b2c3e4-e5f6-7890-acsd-ef1234567891", "amelia.knight@example.org", true, true, null, "AMELIA.KNIGHT@EXAMPLE.ORG", "AMELIA.KNIGHT", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e11", false, "amelia.knight" },
                    { "1a61454c-5b83-4aab-8661-96d6dffbee30", 0, "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "whoLetTheDogsOut@barker.com", true, true, null, "WHOLETTHEDOGSOUT@BARKER.COM", "COOPER.BARKER", "AQAAAAEAACcQAAAAEExamplePasswordHash==", null, false, "d12ef04d-5b83-4aab-8661-567ffb12e15", false, "Cooper.Barker" }
                });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "Categories",
                columns: new[] { "CategoryID", "CategoryName" },
                values: new object[,]
                {
                    { new Guid("0950f1d0-5c03-4f3a-9015-c4bb3c0e7620"), "Cultural Understanding" },
                    { new Guid("25487e1f-b167-4666-a20c-dec2e4b5f413"), "Sustainable Development" },
                    { new Guid("26c867f2-48c6-4bd5-b36a-9f7325431ad3"), "Effective Governance" },
                    { new Guid("3ce7d7d2-176d-4b72-8d98-4b97b49ed0c1"), "Resilience and Adaptability" },
                    { new Guid("81f910e0-39a4-4b44-88ca-fd3c30af4a25"), "Global Cooperation" },
                    { new Guid("a8fb4691-8c1f-4e7d-b315-b042097e6395"), "Equitable Access" },
                    { new Guid("d2c7a605-a621-4b14-8d51-e2df0cecae1a"), "Education and Awareness" },
                    { new Guid("f5c35e6a-8c4f-4556-b6c1-4448b26d1bcb"), "Innovation and Technology" }
                });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "Scopes",
                columns: new[] { "ScopeID", "ScopeName" },
                values: new object[,]
                {
                    { new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Global" },
                    { new Guid("b2d8f1a7-e4c9-3b6a-8d5f-7e6c9d8b3a2f"), "National" },
                    { new Guid("c3b9a2d8-f1e7-6c5b-4d3a-2f1e7d8c9b6a"), "Local" },
                    { new Guid("d4c9b3a2-f8e7-1d6c-5b4a-3f2e1d9c8b7a"), "Individual" }
                });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "Issues",
                columns: new[] { "IssueID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[,]
                {
                    { new Guid("4aebb16c-b474-4c14-9e5e-4548134cadc8"), "1a61454c-5b83-4aab-8661-96d6dffbee30", null, "Discussion on modern urban planning approaches for sustainable and livable cities.", 1, new DateTime(2024, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("b2d8f1a7-e4c9-3b6a-8d5f-7e6c9d8b3a2f"), "Urban Planning Innovations" },
                    { new Guid("b3a72e5d-7c18-4e9f-8d24-67a2c6f35b1d"), "1a61454c-5b83-4aab-8661-96d6dffbe31", null, "The world is experiencing a biodiversity crisis, with thousands of species teetering on the edge of extinction.", 1, new DateTime(2024, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Critical Decline of Endangered Species" },
                    { new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), "1a61454c-5b83-4aab-8661-96d6dffbee30", null, "A issue to discuss practical solutions to climate change at individual and policy levels.", 1, new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Climate Change Solutions" },
                    { new Guid("c246d67c-427c-40f9-8bb2-b0834e473f7b"), "1a61454c-5b83-4aab-8661-96d6dffbe31", null, "Strategies for transitioning to renewable energy sources at community and national levels.", 1, new DateTime(2024, 3, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc"), new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b"), "Renewable Energy Transition" },
                    { new Guid("e5d8f6a9-3b7c-42e1-9d85-7f63a4b5c28d"), "1a61454c-5b83-4aab-8661-96d6dffbe31", null, "The Southern Resident orca population has dropped from 88 individuals in 2010 to just 74 as of late 2024. This decline is attributed to a combination of factors, including reduced prey availability, pollution, and vessel traffic.", 1, new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b3a72e5d-7c18-4e9f-8d24-67a2c6f35b1d"), new Guid("b2d8f1a7-e4c9-3b6a-8d5f-7e6c9d8b3a2f"), "Decline of the Southern Resident orca population" }
                });

            migrationBuilder.InsertData(
                schema: "issues",
                table: "IssuesCategories",
                columns: new[] { "CategoryID", "IssueID" },
                values: new object[,]
                {
                    { new Guid("25487e1f-b167-4666-a20c-dec2e4b5f413"), new Guid("4aebb16c-b474-4c14-9e5e-4548134cadc8") },
                    { new Guid("25487e1f-b167-4666-a20c-dec2e4b5f413"), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc") },
                    { new Guid("26c867f2-48c6-4bd5-b36a-9f7325431ad3"), new Guid("4aebb16c-b474-4c14-9e5e-4548134cadc8") },
                    { new Guid("3ce7d7d2-176d-4b72-8d98-4b97b49ed0c1"), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc") },
                    { new Guid("81f910e0-39a4-4b44-88ca-fd3c30af4a25"), new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc") },
                    { new Guid("f5c35e6a-8c4f-4556-b6c1-4448b26d1bcb"), new Guid("4aebb16c-b474-4c14-9e5e-4548134cadc8") },
                    { new Guid("25487e1f-b167-4666-a20c-dec2e4b5f413"), new Guid("c246d67c-427c-40f9-8bb2-b0834e473f7b") },
                    { new Guid("81f910e0-39a4-4b44-88ca-fd3c30af4a25"), new Guid("c246d67c-427c-40f9-8bb2-b0834e473f7b") },
                    { new Guid("f5c35e6a-8c4f-4556-b6c1-4448b26d1bcb"), new Guid("c246d67c-427c-40f9-8bb2-b0834e473f7b") }
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
                schema: "issues",
                table: "Comments",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_BlockedContentID",
                schema: "issues",
                table: "Comments",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IssueID",
                schema: "issues",
                table: "Comments",
                column: "IssueID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IssueSolutionID",
                schema: "issues",
                table: "Comments",
                column: "IssueSolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentID",
                schema: "issues",
                table: "Comments",
                column: "ParentCommentID");

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
                name: "IX_Issues_ScopeID",
                schema: "issues",
                table: "Issues",
                column: "ScopeID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuesCategories_IssueID",
                schema: "issues",
                table: "IssuesCategories",
                column: "IssueID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_AuthorID",
                schema: "issues",
                table: "Solutions",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_BlockedContentID",
                schema: "issues",
                table: "Solutions",
                column: "BlockedContentID");

            migrationBuilder.CreateIndex(
                name: "IX_Solutions_IssueID",
                schema: "issues",
                table: "Solutions",
                column: "IssueID");

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
                name: "IX_UserHistory_IssueSolutionID",
                schema: "users",
                table: "UserHistory",
                column: "IssueSolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserHistory_UserID",
                schema: "users",
                table: "UserHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_IssueID",
                schema: "issues",
                table: "UserVotes",
                column: "IssueID");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_SolutionID",
                schema: "issues",
                table: "UserVotes",
                column: "SolutionID");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_UserCommentCommentID",
                schema: "issues",
                table: "UserVotes",
                column: "UserCommentCommentID");

            migrationBuilder.CreateIndex(
                name: "IX_UserVotes_UserID",
                schema: "issues",
                table: "UserVotes",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "IssuesCategories",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "UserHistory",
                schema: "users");

            migrationBuilder.DropTable(
                name: "UserVotes",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "Comments",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "Solutions",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "Issues",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BlockedContent",
                schema: "issues");

            migrationBuilder.DropTable(
                name: "Scopes",
                schema: "issues");
        }
    }
}
