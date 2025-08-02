using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    { new Guid("9e8f7a6b-5c4d-4e2f-1a0b-8c7b6a5f4e3d"), null, new DateTime(2024, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("b9a7c8d6-5e4f-48b3-90a1-2d3e4f5c6b7a"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("a0b1c2d9-8e5f-40a6-c4d1-b7f0e8a5d3c2"), null, new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 8 },
                    { new Guid("a1b7f6e5-8d32-47c0-b965-2f48a3b1c7d6"), null, new DateTime(2024, 10, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 10 },
                    { new Guid("a1f0e9d8-7c64-41b5-b309-4e82f7a5b1c0"), null, new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 9 },
                    { new Guid("a3d5c4b2-0e9f-43a8-b7b6-2c1d0e9f8a7b"), null, new DateTime(2024, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("a3f2e1d0-9c86-43b7-b521-6e04f9a7b3c2"), null, new DateTime(2024, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("a5b1f0e9-2d76-41c4-b309-6f82a7b5c1d0"), null, new DateTime(2024, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 9 },
                    { new Guid("a6b7c8d5-4e1f-46a2-c0d7-b3f6e4a1d9c8"), null, new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 7 },
                    { new Guid("a7b3f2e1-4d98-43c6-b521-8f04a9b7c3d2"), null, new DateTime(2024, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 10 },
                    { new Guid("a7f6e5d4-3c20-47b1-b965-0e48f3a1b7c6"), null, new DateTime(2024, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 9 },
                    { new Guid("a9d1c0b8-6e5f-49a4-b3b2-8c7d6e5f4a3b"), null, new DateTime(2024, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 8 },
                    { new Guid("b0e2d1c9-7f6a-40b5-b4c3-9d8e7f6a5b4c"), null, new DateTime(2024, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 9 },
                    { new Guid("b1c2d3e0-9f6a-41b7-d5e2-c8a1f9b6e4d3"), null, new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 7 },
                    { new Guid("b2a1f0e9-8d75-42c6-b410-5f93a8b6c2d1"), null, new DateTime(2024, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("b2c8a7f6-9e43-48d1-b076-3a59b4c2d8e7"), null, new DateTime(2024, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("b4a3f2e1-0d97-44c8-b632-7f15a0b8c4d3"), null, new DateTime(2024, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 10 },
                    { new Guid("b4e6d5c3-1f0a-44b9-a8c7-3d2e1f0a9b8c"), null, new DateTime(2024, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("b6c2a1f0-3e87-42d5-b410-7a93b8c6d2e1"), null, new DateTime(2024, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 10 },
                    { new Guid("b7c8d9e6-5f2a-47b3-d1e8-c4a7f5b2e0d9"), null, new DateTime(2024, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 8 },
                    { new Guid("b8a7f6e5-4d31-48c2-b076-1f59a4b2c8d7"), null, new DateTime(2024, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 8 },
                    { new Guid("b8e0d9c7-5f4a-48b3-a2c1-7d6e5f4a3b2c"), null, new DateTime(2024, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("c1f3e2d0-8a7b-41c6-b5d4-0e9f8a7b6c5d"), null, new DateTime(2024, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("c2d3e4f1-0a7b-42c8-e6f3-d9b2a0c7f5e4"), null, new DateTime(2024, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("c3b2a1f0-9e86-43d7-b521-6a04b9c7d3e2"), null, new DateTime(2024, 10, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("c3d9b8a7-0f54-49e2-b187-4b60c5d3e9f8"), null, new DateTime(2024, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 8 },
                    { new Guid("c5f7e6d4-2a1b-45c0-b9d8-4e3f2a1b0c9d"), null, new DateTime(2024, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 9 },
                    { new Guid("c7d3b2a1-4f98-43e6-a521-8b04c9d7e3f2"), null, new DateTime(2024, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 9 },
                    { new Guid("c8d9e0f7-6a3b-48c4-e2f9-d5b8a6c3f1e0"), null, new DateTime(2024, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("c9b8a7f6-5e42-49d3-b187-2a60b5c3d9e8"), null, new DateTime(2024, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("c9f1e0d8-6a5b-49c4-b3d2-8e7f6a5b4c3d"), null, new DateTime(2024, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("d0a2f1e9-7b6c-40d5-b4e3-9f8a7b6c5d4e"), null, new DateTime(2024, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee32"), 10 },
                    { new Guid("d0c9b8a7-6f53-40e4-b298-3b71c6d4e0f9"), null, new DateTime(2024, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("d2a4f3e1-9b8c-42d7-b6e5-1f0a9b8c7d6e"), null, new DateTime(2024, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("d3e4f5a2-1b8c-43d9-f7a4-e0c3b1d8a6f5"), null, new DateTime(2024, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("d4c3b2a1-0f97-44e8-a632-7b15c0d8e4f3"), null, new DateTime(2024, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("d4e0c9b8-1a65-40f3-b298-5c71d6e4f0a9"), null, new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("d6a8f7e5-3b2c-46d1-b0e9-5f4a3b2c1d0e"), null, new DateTime(2024, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 8 },
                    { new Guid("d6e7f8a9-b0c1-47d2-93e4-5f8c7b6a9d0e"), null, new DateTime(2024, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("c5d2e3f2-b1a0-47c9-8d67-5f2e3b4a1d90"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 8 },
                    { new Guid("d8e4c3b2-5a09-44f7-b632-9c15d0e8f4a3"), null, new DateTime(2024, 10, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 8 },
                    { new Guid("d9e0f1a8-7b4c-49d5-f3a0-e6c9b7d4a2f1"), null, new DateTime(2024, 5, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 10 },
                    { new Guid("e1b3a2f0-8c7d-41e6-b5f4-0a9b8c7d6e5f"), null, new DateTime(2024, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee33"), 9 },
                    { new Guid("e1d0c9b8-7a64-41f5-b309-4c82d7e5f1a0"), null, new DateTime(2024, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("e3f9d8c7-0b54-49a2-b187-4d60e5f3a9b8"), null, new DateTime(2024, 10, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("e4f5a6b3-2c9d-44e0-a8b5-f1d4c2e9b7a6"), null, new DateTime(2024, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("e5d4c3b2-1a08-45f9-b743-8c26d1e9f5a4"), null, new DateTime(2024, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee35"), 9 },
                    { new Guid("e5f1d0c9-2b76-41a4-b309-6d82e7f5a1b0"), null, new DateTime(2024, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 8 },
                    { new Guid("e7b9a8f6-4c3d-47e2-b1f0-6a5b4c3d2e1f"), null, new DateTime(2024, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee39"), 9 },
                    { new Guid("e8f9a0b7-6c3d-48e4-a2b9-f5d8c6e3a1b0"), null, new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 9 },
                    { new Guid("e9f5d4c3-6b10-45a8-b743-0d26e1f9a5b4"), null, new DateTime(2024, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 9 },
                    { new Guid("f0a6e5d4-7c21-46b9-b854-1e37f2a0b6c5"), null, new DateTime(2024, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee37"), 8 },
                    { new Guid("f0e9d8c7-6b53-40a4-b298-3d71e6f4a0b9"), null, new DateTime(2024, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 10 },
                    { new Guid("f2c4b3a1-9d8e-42f7-b6a5-1b0c9d8e7f6a"), null, new DateTime(2024, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee34"), 8 },
                    { new Guid("f2e1d0c9-8b75-42a6-b410-5d93e8f6a2b1"), null, new DateTime(2024, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 8 },
                    { new Guid("f4a0e9d8-1c65-40b3-b298-5e71f6a4b0c9"), null, new DateTime(2024, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 8 },
                    { new Guid("f5a6b7c4-3d0e-45f1-b9c6-a2e5d3f0c8b7"), null, new DateTime(2024, 5, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee38"), 9 },
                    { new Guid("f6a2e1d0-3c87-42b5-b410-7e93f8a6b2c1"), null, new DateTime(2024, 10, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d2e8c7b5-9a43-48f1-b076-3c59d4a2e8f0"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 9 },
                    { new Guid("f6e5d4c3-2b19-46a0-b854-9d37e2f0a6b5"), null, new DateTime(2024, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("e9d8c7b6-5a42-49f3-b187-2c60d5a3e9f1"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee36"), 10 },
                    { new Guid("f8c0b9a7-5d4e-48f3-b2a1-7b6c5d4e3f2a"), null, new DateTime(2024, 10, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("a7d9c8b6-4e3f-47a2-95d1-6b8c0a5e3f2d"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 10 },
                    { new Guid("f9a0b1c8-7d4e-49f5-b3c0-a6e9d7f4b2c1"), null, new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("d7e9f8a6-5b2c-47d3-91a8-e4c7b5d2f10e"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee31"), 10 }
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
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a0b1c2d9-8e5f-40a6-c4d1-b7f0e8a5d3c2"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a1b7f6e5-8d32-47c0-b965-2f48a3b1c7d6"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a1f0e9d8-7c64-41b5-b309-4e82f7a5b1c0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a3d5c4b2-0e9f-43a8-b7b6-2c1d0e9f8a7b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a3f2e1d0-9c86-43b7-b521-6e04f9a7b3c2"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a5b1f0e9-2d76-41c4-b309-6f82a7b5c1d0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a6b7c8d5-4e1f-46a2-c0d7-b3f6e4a1d9c8"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a7b3f2e1-4d98-43c6-b521-8f04a9b7c3d2"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a7f6e5d4-3c20-47b1-b965-0e48f3a1b7c6"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("a9d1c0b8-6e5f-49a4-b3b2-8c7d6e5f4a3b"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b0e2d1c9-7f6a-40b5-b4c3-9d8e7f6a5b4c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b1c2d3e0-9f6a-41b7-d5e2-c8a1f9b6e4d3"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b2a1f0e9-8d75-42c6-b410-5f93a8b6c2d1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b2c8a7f6-9e43-48d1-b076-3a59b4c2d8e7"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b4a3f2e1-0d97-44c8-b632-7f15a0b8c4d3"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b4e6d5c3-1f0a-44b9-a8c7-3d2e1f0a9b8c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b6c2a1f0-3e87-42d5-b410-7a93b8c6d2e1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b7c8d9e6-5f2a-47b3-d1e8-c4a7f5b2e0d9"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b8a7f6e5-4d31-48c2-b076-1f59a4b2c8d7"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("b8e0d9c7-5f4a-48b3-a2c1-7d6e5f4a3b2c"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c1f3e2d0-8a7b-41c6-b5d4-0e9f8a7b6c5d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c2d3e4f1-0a7b-42c8-e6f3-d9b2a0c7f5e4"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c3b2a1f0-9e86-43d7-b521-6a04b9c7d3e2"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c3d9b8a7-0f54-49e2-b187-4b60c5d3e9f8"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c5f7e6d4-2a1b-45c0-b9d8-4e3f2a1b0c9d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c7d3b2a1-4f98-43e6-a521-8b04c9d7e3f2"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c8d9e0f7-6a3b-48c4-e2f9-d5b8a6c3f1e0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c9b8a7f6-5e42-49d3-b187-2a60b5c3d9e8"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("c9f1e0d8-6a5b-49c4-b3d2-8e7f6a5b4c3d"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d0a2f1e9-7b6c-40d5-b4e3-9f8a7b6c5d4e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d0c9b8a7-6f53-40e4-b298-3b71c6d4e0f9"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d2a4f3e1-9b8c-42d7-b6e5-1f0a9b8c7d6e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d3e4f5a2-1b8c-43d9-f7a4-e0c3b1d8a6f5"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d4c3b2a1-0f97-44e8-a632-7b15c0d8e4f3"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d4e0c9b8-1a65-40f3-b298-5c71d6e4f0a9"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d6a8f7e5-3b2c-46d1-b0e9-5f4a3b2c1d0e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d6e7f8a9-b0c1-47d2-93e4-5f8c7b6a9d0e"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d8e4c3b2-5a09-44f7-b632-9c15d0e8f4a3"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("d9e0f1a8-7b4c-49d5-f3a0-e6c9b7d4a2f1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e1b3a2f0-8c7d-41e6-b5f4-0a9b8c7d6e5f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e1d0c9b8-7a64-41f5-b309-4c82d7e5f1a0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e3f9d8c7-0b54-49a2-b187-4d60e5f3a9b8"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e4f5a6b3-2c9d-44e0-a8b5-f1d4c2e9b7a6"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e5d4c3b2-1a08-45f9-b743-8c26d1e9f5a4"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e5f1d0c9-2b76-41a4-b309-6d82e7f5a1b0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e7b9a8f6-4c3d-47e2-b1f0-6a5b4c3d2e1f"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e8f9a0b7-6c3d-48e4-a2b9-f5d8c6e3a1b0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("e9f5d4c3-6b10-45a8-b743-0d26e1f9a5b4"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f0a6e5d4-7c21-46b9-b854-1e37f2a0b6c5"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f0e9d8c7-6b53-40a4-b298-3d71e6f4a0b9"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f2c4b3a1-9d8e-42f7-b6a5-1b0c9d8e7f6a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f2e1d0c9-8b75-42a6-b410-5d93e8f6a2b1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f4a0e9d8-1c65-40b3-b298-5e71f6a4b0c9"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f5a6b7c4-3d0e-45f1-b9c6-a2e5d3f0c8b7"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f6a2e1d0-3c87-42b5-b410-7e93f8a6b2c1"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f6e5d4c3-2b19-46a0-b854-9d37e2f0a6b5"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f8c0b9a7-5d4e-48f3-b2a1-7b6c5d4e3f2a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f9a0b1c8-7d4e-49f5-b3c0-a6e9d7f4b2c1"));
        }
    }
}
