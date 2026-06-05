using System;
using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HWAY.Payroll.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260605000000_MoveHireDateToEmployeesAddBirthDateAndEiExempt")]
    public partial class MoveHireDateToEmployeesAddBirthDateAndEiExempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isNpgsql = migrationBuilder.ActiveProvider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);

            migrationBuilder.AddColumn<DateTime>(
                name: "Hire Date",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date of Birth",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EI_Exempt",
                table: "Employees",
                type: isNpgsql ? "boolean" : "bit",
                nullable: false,
                defaultValue: false);

            if (isNpgsql)
            {
                migrationBuilder.Sql(
                    """
                    update "Employees" as e
                    set "Hire Date" = pr."Hire Date"
                    from (
                        select "Employee #", min("Hire Date") as "Hire Date"
                        from "PayrollRecords"
                        where "Hire Date" is not null
                        group by "Employee #"
                    ) as pr
                    where e."Employee #" = pr."Employee #";
                    """);
            }
            else
            {
                migrationBuilder.Sql(
                    """
                    update e
                    set [Hire Date] = pr.[Hire Date]
                    from [Employees] as e
                    inner join (
                        select [Employee #], min([Hire Date]) as [Hire Date]
                        from [PayrollRecords]
                        where [Hire Date] is not null
                        group by [Employee #]
                    ) as pr on e.[Employee #] = pr.[Employee #];
                    """);
            }

            migrationBuilder.DropColumn(
                name: "Hire Date",
                table: "PayrollRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var isNpgsql = migrationBuilder.ActiveProvider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);

            migrationBuilder.AddColumn<DateTime>(
                name: "Hire Date",
                table: "PayrollRecords",
                type: "date",
                nullable: true);

            if (isNpgsql)
            {
                migrationBuilder.Sql(
                    """
                    update "PayrollRecords" as pr
                    set "Hire Date" = e."Hire Date"
                    from "Employees" as e
                    where pr."Employee #" = e."Employee #";
                    """);
            }
            else
            {
                migrationBuilder.Sql(
                    """
                    update pr
                    set [Hire Date] = e.[Hire Date]
                    from [PayrollRecords] as pr
                    inner join [Employees] as e on pr.[Employee #] = e.[Employee #];
                    """);
            }

            migrationBuilder.DropColumn(
                name: "EI_Exempt",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Date of Birth",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Hire Date",
                table: "Employees");
        }
    }
}
