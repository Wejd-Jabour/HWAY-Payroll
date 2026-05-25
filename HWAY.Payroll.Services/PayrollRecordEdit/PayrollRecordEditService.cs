using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.PayrollRecordEdit;

public class PayrollRecordEditService : IPayrollRecordEditService
{
    private readonly AppDbContext _dbContext;

    public PayrollRecordEditService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpdatePayrollRecordAsync(int payrollRecordId, PayrollRecordEditDto dto)
    {
        var record = await _dbContext.PayrollRecords
            .Where(r => r.Id == payrollRecordId)
            .FirstOrDefaultAsync();

        if (record == null)
            throw new InvalidOperationException($"Payroll record with Id {payrollRecordId} not found.");

        var netPay = CalculateNetPay(
            dto.Salary,
            dto.Gross,
            dto.Vacation,
            dto.Tax1,
            dto.Cpp1,
            dto.Cpp2,
            dto.Ei);

        record.EmployeeName = dto.EmployeeName;
        record.EmployeeNumber = dto.EmployeeNumber;
        record.RpNumber = dto.RpNumber;
        record.Salary = dto.Salary;
        record.Rate = dto.Rate;
        record.RegularHours = dto.RegularHours;
        record.Gross = dto.Gross;
        record.Vacation = dto.Vacation;
        record.Tax1 = dto.Tax1;
        record.Cpp1 = dto.Cpp1;
        record.Cpp2 = dto.Cpp2;
        record.Ei = dto.Ei;
        record.OtherDeduction = dto.OtherDeduction;
        record.NetPay = netPay;

        record.GrossYtd += dto.Gross;
        record.NetYtd += netPay;
        record.TaxYtd += dto.Tax1;
        record.Cpp1Ytd += dto.Cpp1;
        record.Cpp2Ytd += dto.Cpp2;
        record.EiYtd += dto.Ei;
        record.OtherDeductionYtd += dto.OtherDeduction;
        record.VacationYtd += dto.Vacation;

        await _dbContext.SaveChangesAsync();
    }

    private static decimal CalculateNetPay(
        decimal salary,
        decimal gross,
        decimal vacation,
        decimal tax1,
        decimal cpp1,
        decimal cpp2,
        decimal ei)
    {
        var totalDeductions = tax1 + cpp1 + cpp2 + ei;

        return salary > 0
            ? salary - totalDeductions
            : gross + vacation - totalDeductions;
    }
}
