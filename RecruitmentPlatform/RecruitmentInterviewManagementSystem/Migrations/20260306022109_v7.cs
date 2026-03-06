using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentInterviewManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class v7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "EmployerProfiles");

            migrationBuilder.AddColumn<string>(
                name: "TemplateId",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "CVs");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "EmployerProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
