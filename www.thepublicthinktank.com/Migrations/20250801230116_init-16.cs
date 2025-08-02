using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace atlas_the_public_think_tank.Migrations
{
    /// <inheritdoc />
    public partial class init16 : Migration
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
                    { new Guid("01278901-3cd4-e5f6-789a-bcdef0123456"), null, new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30"), 10 },
                    { new Guid("12389012-4de5-f607-89ab-cdef01234567"), null, new DateTime(2024, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee46"), 11 },
                    { new Guid("23490123-5ef6-0789-abcd-ef012345678a"), null, new DateTime(2024, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee47"), 12 },
                    { new Guid("34501234-6f07-89ab-cdef-0123456789ab"), null, new DateTime(2024, 1, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee48"), 12 },
                    { new Guid("45612345-7a89-bcde-f012-3456789abcde"), null, new DateTime(2024, 1, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee49"), 13 },
                    { new Guid("abc12345-de67-89f0-1234-56789abcdef0"), null, new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee40"), 9 },
                    { new Guid("bcd23456-ef78-90a1-2345-6789abcdef01"), null, new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee41"), 9 },
                    { new Guid("cde34567-f890-a1b2-3456-789abcdef012"), null, new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee42"), 10 },
                    { new Guid("def45678-90a1-b2c3-4567-89abcdef0123"), null, new DateTime(2024, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee43"), 8 },
                    { new Guid("ef056789-1ab2-c3d4-5678-9abcdef01234"), null, new DateTime(2024, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee44"), 9 },
                    { new Guid("f0167890-2bc3-d4e5-6789-abcdef012345"), null, new DateTime(2024, 1, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new Guid("8f6b5d24-c7a1-4e3f-9b8c-d15e83f62a19"), new Guid("1a61454c-5b83-4aab-8661-96d6dff2ee45"), 9 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("01278901-3cd4-e5f6-789a-bcdef0123456"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("12389012-4de5-f607-89ab-cdef01234567"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("23490123-5ef6-0789-abcd-ef012345678a"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("34501234-6f07-89ab-cdef-0123456789ab"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("45612345-7a89-bcde-f012-3456789abcde"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("abc12345-de67-89f0-1234-56789abcdef0"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("bcd23456-ef78-90a1-2345-6789abcdef01"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("cde34567-f890-a1b2-3456-789abcdef012"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("def45678-90a1-b2c3-4567-89abcdef0123"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("ef056789-1ab2-c3d4-5678-9abcdef01234"));

            migrationBuilder.DeleteData(
                schema: "solutions",
                table: "SolutionVotes",
                keyColumn: "VoteID",
                keyValue: new Guid("f0167890-2bc3-d4e5-6789-abcdef012345"));
        }
    }
}
