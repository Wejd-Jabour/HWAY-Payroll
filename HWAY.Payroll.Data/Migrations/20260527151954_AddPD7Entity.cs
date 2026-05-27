using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HWAY.Payroll.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPD7Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PD7",
                columns: table => new
                {
                    RP = table.Column<string>(name: "RP#", type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Gross = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PD7Total = table.Column<decimal>(name: "PD7 Total", type: "decimal(18,2)", nullable: false),
                    ActiveEmployees = table.Column<int>(name: "Active Employees", type: "int", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Transfer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Received = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeeCheck = table.Column<bool>(name: "Fee Check", type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PD7", x => new { x.RP, x.Month, x.Year });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PD7");
        }
    }
}
