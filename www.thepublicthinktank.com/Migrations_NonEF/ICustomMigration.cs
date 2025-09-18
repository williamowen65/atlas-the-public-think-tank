using Microsoft.EntityFrameworkCore.Migrations;

namespace atlas_the_public_think_tank.Migrations_NonEF
{
    public interface ICustomMigration
    {
        void Up(MigrationBuilder migrationBuilder);
        void Down(MigrationBuilder migrationBuilder);
    }
}
