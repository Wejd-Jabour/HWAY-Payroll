using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Domain;
using HWAY.Payroll.Services;
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
            .Include(r => r.Employer)
            .Where(r => r.Id == payrollRecordId)
            .FirstOrDefaultAsync();

        if (record == null)
            throw new InvalidOperationException($"Payroll record with Id {payrollRecordId} not found.");

        record.Period = CalculatePeriod(record.Employer?.EmployerGroup, DateTime.Today);
        var salary = dto.Salary;
        var rate = salary > 0 ? 0 : dto.Rate;
        var payrollYear = DateTime.Today.Year;
        var cpp1 = PayrollDeductionCaps.ClampCpp1(dto.Cpp1, record.Cpp1Ytd, payrollYear);
        var cpp2 = PayrollDeductionCaps.ClampCpp2(dto.Cpp2, record.Cpp2Ytd, payrollYear);
        var ei = PayrollDeductionCaps.ClampEi(dto.Ei, record.EiYtd, payrollYear);
        var vacationSetting = VacationSettings.Normalize(dto.VacationSetting);
        var vacationUsed = VacationSettings.IsPaidOut(vacationSetting) ? dto.Vacation : 0m;

        var netPay = CalculateNetPay(
            dto.Gross,
            dto.Vacation,
            vacationSetting,
            dto.Tax1,
            cpp1,
            cpp2,
            ei,
            dto.OtherDeduction);

        record.EmployeeName = dto.EmployeeName;
        record.EmployeeNumber = dto.EmployeeNumber;
        record.RpNumber = dto.RpNumber;
        record.Salary = salary;
        record.Rate = rate;
        record.RegularHours = dto.RegularHours;
        record.OvertimeHours = dto.OvertimeHours;
        record.Gross = dto.Gross;
        record.Vacation = dto.Vacation;
        record.VacationSetting = vacationSetting;
        record.Tax1 = dto.Tax1;
        record.Cpp1 = cpp1;
        record.Cpp2 = cpp2;
        record.Ei = ei;
        record.OtherDeduction = dto.OtherDeduction;
        record.NetPay = netPay;

        record.GrossYtd += dto.Gross;
        record.NetYtd += netPay;
        record.TaxYtd += dto.Tax1;
        record.Cpp1Ytd += cpp1;
        record.Cpp2Ytd += cpp2;
        record.EiYtd += ei;
        record.OtherDeductionYtd += dto.OtherDeduction;
        var updatedVacationYtd = record.VacationYtd + dto.Vacation;
        var updatedVacationYtdUsed = record.VacationYtdUsed + vacationUsed;

        record.VacationYtd = updatedVacationYtd;
        record.VacationYtdUsed = updatedVacationYtdUsed;
        record.VacationYtdBalance = updatedVacationYtd - updatedVacationYtdUsed;

        await _dbContext.SaveChangesAsync();
    }

    private static int CalculatePeriod(string? employerGroup, DateTime currentDate)
    {
        var group = (employerGroup ?? "A").Trim().ToUpperInvariant();

        return group switch
        {
            // Semi-monthly: 2 periods per month (01-24 per year)
            "B" => (currentDate.Month - 1) * 2 + (currentDate.Day <= 15 ? 1 : 2),
            // Biweekly: ~26 periods per year
            "C" or "D" => ((currentDate.DayOfYear - 1) / 14) + 1,
            // Monthly: 12 periods per year (01-12 per year)
            _ => currentDate.Month,
        };
    }

    private static decimal CalculateNetPay(
        decimal gross,
        decimal vacation,
        string vacationSetting,
        decimal tax1,
        decimal cpp1,
        decimal cpp2,
        decimal ei,
        decimal otherDeduction)
    {
        var totalDeductions = tax1 + cpp1 + cpp2 + ei + otherDeduction;
        var paidVacation = VacationSettings.IsPaidOut(vacationSetting) ? vacation : 0m;

        return gross + paidVacation - totalDeductions;
    }
}
