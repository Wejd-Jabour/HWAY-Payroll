using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HWAY.Payroll.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollRecordVacationSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "vacation_setting",
                table: "PayrollRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Accumulate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "vacation_setting",
                table: "PayrollRecords");
        }
    }
}
