using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "scopes",
                table: "Scopes",
                columns: new[] { "ScopeID", "Boundaries", "Domains", "EntityTypes", "Scales", "Timeframes" },
                values: new object[] { new Guid("a059b123-f631-4774-a866-33ba0bb415b5"), "[]", "[]", "[]", "[6]", "[]" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "Solutions",
                columns: new[] { "SolutionID", "AuthorID", "BlockedContentID", "Content", "ContentStatus", "CreatedAt", "ModifiedAt", "ParentIssueID", "ScopeID", "Title" },
                values: new object[] { new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), null, "Second Chance Hiring Incentives represent a comprehensive approach to addressing employment barriers for individuals transitioning from homelessness, incarceration, or extended unemployment. These programs create pathways to stable employment—a critical factor in securing and maintaining housing.\n\nThe solution involves multi-faceted incentives for employers who hire qualified candidates with barriers to employment. Tax credits form the foundation, offering businesses direct financial benefits for each eligible employee hired and retained. Wage subsidies complement tax incentives by offsetting initial training costs during the critical onboarding period when productivity may be developing. Bonding programs provide insurance protection against potential employee dishonesty, removing a significant concern for employers considering candidates with criminal histories.\n\nBeyond financial incentives, this approach includes support services that benefit both employers and employees: specialized job coaches who provide ongoing mentorship; liaison services that help navigate workplace challenges; and training grants that fund skill development tailored to specific industry needs. Recognition programs highlight businesses demonstrating inclusive hiring practices, creating positive public relations opportunities.\n\nImplementation requires collaboration between government agencies, community organizations, and the business community. Streamlined application processes and clear eligibility guidelines are essential to encourage employer participation. Success metrics should track not only initial placements but long-term retention and career advancement.\n\nWhen properly structured, Second Chance Hiring Incentives create mutual benefits: employers gain motivated, loyal employees and financial advantages, while vulnerable individuals secure economic self-sufficiency and stable housing. Communities benefit from reduced homelessness, decreased recidivism, expanded tax bases, and the economic multiplier effects of increased employment.", 1, new DateTime(2024, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e47a95c8-939e-4b5a-a054-f7c127db4eb3"), new Guid("a059b123-f631-4774-a866-33ba0bb415b5"), "Second Chance Hiring Incentives for Employers" });

            migrationBuilder.InsertData(
                schema: "solutions",
                table: "SolutionVotes",
                columns: new[] { "VoteID", "AppUserId", "CreatedAt", "ModifiedAt", "SolutionID", "UserID", "VoteValue" },
                values: new object[,]
                {
                    { new Guid("0f9a8b7c-6d5e-4f3a-2b1c-9d8c7b6a5f4e"), null, new DateTime(2024, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("1a0b9c8d-7e6f-4a4b-3c2d-0e9d8c7b6a5f"), null, new DateTime(2024, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 8 },
                    { new Guid("2b1c0d9e-8f7a-4b5c-4d3e-1f0e9d8c7b6a"), null, new DateTime(2024, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 9 },
                    { new Guid("3c2d1e0f-9a8b-4c6d-5e4f-2a1f0e9d8c7b"), null, new DateTime(2024, 9, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 7 },
                    { new Guid("4d3e2f1a-0b9c-4d7e-6f5a-3b2a1f0e9d8c"), null, new DateTime(2024, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 8 },
                    { new Guid("5a4b3c2d-1e0f-4a8b-9c7d-6e5f4d3c2b1a"), null, new DateTime(2024, 9, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("5e4f3a2b-1c0d-4e8f-7a6b-4c3b2a1f0e9d"), null, new DateTime(2024, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 9 },
                    { new Guid("6b5c4d3e-2f1a-4b9c-8d7e-5f4e3d2c1b0a"), null, new DateTime(2024, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("6f5a4b3c-2d1e-4f9a-8b7c-5d4c3b2a1f0e"), null, new DateTime(2024, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("7a6b5c4d-3e2f-4a0b-9c8d-6e5d4c3b2a1f"), null, new DateTime(2024, 9, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 7 },
                    { new Guid("7c6d5e4f-3a2b-4c0d-9e8f-6a5f4e3d2c1b"), null, new DateTime(2024, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 9 },
                    { new Guid("8b7c6d5e-4f3a-4b1c-0d9e-7f6e5d4c3b2a"), null, new DateTime(2024, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("8d7e6f5a-4b3c-4d1e-0f9a-7b6a5f4e3d2c"), null, new DateTime(2024, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 7 },
                    { new Guid("9c8d7e6f-5a4b-4c2d-1e0f-8a7f6e5d4c3b"), null, new DateTime(2024, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("9e8f7a6b-5c4d-4e2f-1a0b-8c7b6a5f4e3d"), null, new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("0f9a8b7c-6d5e-4f3a-2b1c-9d8c7b6a5f4e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("1a0b9c8d-7e6f-4a4b-3c2d-0e9d8c7b6a5f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("2b1c0d9e-8f7a-4b5c-4d3e-1f0e9d8c7b6a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("3c2d1e0f-9a8b-4c6d-5e4f-2a1f0e9d8c7b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("4d3e2f1a-0b9c-4d7e-6f5a-3b2a1f0e9d8c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("5a4b3c2d-1e0f-4a8b-9c7d-6e5f4d3c2b1a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("5e4f3a2b-1c0d-4e8f-7a6b-4c3b2a1f0e9d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("6b5c4d3e-2f1a-4b9c-8d7e-5f4e3d2c1b0a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("6f5a4b3c-2d1e-4f9a-8b7c-5d4c3b2a1f0e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("7a6b5c4d-3e2f-4a0b-9c8d-6e5d4c3b2a1f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("7c6d5e4f-3a2b-4c0d-9e8f-6a5f4e3d2c1b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("8b7c6d5e-4f3a-4b1c-0d9e-7f6e5d4c3b2a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("8d7e6f5a-4b3c-4d1e-0f9a-7b6a5f4e3d2c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("9c8d7e6f-5a4b-4c2d-1e0f-8a7f6e5d4c3b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("9e8f7a6b-5c4d-4e2f-1a0b-8c7b6a5f4e3d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "Solutions",
                keyColumn: "SolutionID",
                keyValue: new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"));

            migrationBuilder.DeleteData(
                schema: "scopes",
                table: "Scopes",
                keyColumn: "ScopeID",
                keyValue: new Guid("a059b123-f631-4774-a866-33ba0bb415b5"));
        }
    }
}
