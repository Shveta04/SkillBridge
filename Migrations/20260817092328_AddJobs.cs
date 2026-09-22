using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillBridge.Migrations
{
    public partial class AddJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Jobs table already exists in the database.
            // No database creation is required here.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Do not delete the existing Jobs table.
        }
    }
}