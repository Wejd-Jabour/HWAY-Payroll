using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HWAY.Payroll.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollRecordPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "PayrollRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Period",
                table: "PayrollRecords");
        }
    }
}
