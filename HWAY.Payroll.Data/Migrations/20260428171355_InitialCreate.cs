using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HWAY.Payroll.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Employee = table.Column<string>(name: "Employee #", type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(name: "First Name", type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(name: "Last Name", type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SIN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PostalCode = table.Column<string>(name: "Postal Code", type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EMailAddress = table.Column<string>(name: "E-Mail Address", type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.UniqueConstraint("AK_Employees_Employee #", x => x.Employee);
                });

            migrationBuilder.CreateTable(
                name: "Employers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RP = table.Column<string>(name: "RP#", type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployerName = table.Column<string>(name: "Employer Name", type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Postalcode = table.Column<string>(name: "Postal code", type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmployerGroup = table.Column<string>(name: "Employer Group", type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employers", x => x.Id);
                    table.UniqueConstraint("AK_Employers_RP#", x => x.RP);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Employee = table.Column<string>(name: "Employee #", type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RP = table.Column<string>(name: "RP#", type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeesName = table.Column<string>(name: "Employees Name", type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RHours = table.Column<decimal>(name: "R.Hours", type: "decimal(18,2)", nullable: false),
                    OVHours = table.Column<decimal>(name: "OV.Hours", type: "decimal(18,2)", nullable: false),
                    PublicH = table.Column<decimal>(name: "Public.H", type: "decimal(18,2)", nullable: false),
                    SHHours = table.Column<decimal>(name: "S.H Hours", type: "decimal(18,2)", nullable: false),
                    Vacation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Gross = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TAX1 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CPP1 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CPP2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EI = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Otherdeduction = table.Column<decimal>(name: "Other deduction", type: "decimal(18,2)", nullable: false),
                    NETPAY = table.Column<decimal>(name: "NET PAY", type: "decimal(18,2)", nullable: false),
                    GrossYTD = table.Column<decimal>(name: "Gross-YTD", type: "decimal(18,2)", nullable: false),
                    NETYTD = table.Column<decimal>(name: "NET-YTD", type: "decimal(18,2)", nullable: false),
                    TAXYTD = table.Column<decimal>(name: "TAX -YTD", type: "decimal(18,2)", nullable: false),
                    CPP1YTD = table.Column<decimal>(name: "CPP1-YTD", type: "decimal(18,2)", nullable: false),
                    CPP2YTD = table.Column<decimal>(name: "CPP2-YTD", type: "decimal(18,2)", nullable: false),
                    EIYTD = table.Column<decimal>(name: "EI-YTD", type: "decimal(18,2)", nullable: false),
                    OtherDeductionYTD = table.Column<decimal>(name: "Other Deduction-YTD", type: "decimal(18,2)", nullable: false),
                    VacationYTD = table.Column<decimal>(name: "Vacation -YTD", type: "decimal(18,2)", nullable: false),
                    VacationYTDUsed = table.Column<decimal>(name: "Vacation -YTD-Used", type: "decimal(18,2)", nullable: false),
                    VacationYTDBalance = table.Column<decimal>(name: "Vacation -YTD- Balance", type: "decimal(18,2)", nullable: false),
                    TotalPD7 = table.Column<decimal>(name: "Total PD7", type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_Employees_Employee #",
                        column: x => x.Employee,
                        principalTable: "Employees",
                        principalColumn: "Employee #",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_Employers_RP#",
                        column: x => x.RP,
                        principalTable: "Employers",
                        principalColumn: "RP#",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Employee #",
                table: "Employees",
                column: "Employee #",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employers_RP#",
                table: "Employers",
                column: "RP#",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_Employee #",
                table: "PayrollRecords",
                column: "Employee #");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_RP#",
                table: "PayrollRecords",
                column: "RP#");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollRecords");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Employers");
        }
    }
}
