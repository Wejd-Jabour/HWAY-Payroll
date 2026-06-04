using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HWAY.Payroll.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddisActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is Active",
                table: "Employers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Is Active",
                table: "Employees",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is Active",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "Is Active",
                table: "Employees");
        }
    }
}
