using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Domain;
using HWAY.Payroll.Services;
using HWAY.Payroll.Services.PD7;
using HWAY.Payroll.Services.PayPeriods;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.PayrollRecordEdit;

public class PayrollRecordEditService : IPayrollRecordEditService
{
    private readonly AppDbContext _dbContext;
    private readonly IPayrollDeductionLimitService _deductionLimitService;
    private readonly IPD7LookupService _pd7LookupService;

    public PayrollRecordEditService(
        AppDbContext dbContext,
        IPayrollDeductionLimitService deductionLimitService,
        IPD7LookupService pd7LookupService)
    {
        _dbContext = dbContext;
        _deductionLimitService = deductionLimitService;
        _pd7LookupService = pd7LookupService;
    }

    public async Task<PayrollRecordEditResultDto> UpdatePayrollRecordAsync(int payrollRecordId, PayrollRecordEditDto dto)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            var record = await _dbContext.PayrollRecords
                .Include(r => r.Employer)
                .Include(r => r.Employee)
                .Where(r => r.Id == payrollRecordId)
                .FirstOrDefaultAsync();

            if (record == null)
                throw new InvalidOperationException($"Payroll record with Id {payrollRecordId} not found.");

            if (!string.Equals(record.EmployeeNumber, dto.EmployeeNumber.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Selected employee does not match the payroll record being edited.");

            if (!string.Equals(record.RpNumber, dto.RpNumber.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Selected company does not match the payroll record being edited.");

            var employee = await _dbContext.Employees
                .Where(e => e.EmployeeNumber == record.EmployeeNumber)
                .FirstOrDefaultAsync();

            if (employee == null)
                throw new InvalidOperationException($"Employee with number {record.EmployeeNumber} not found.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var salary = dto.Salary;
            var rate = salary > 0 ? 0 : dto.Rate;
            var payrollYear = DateTime.Today.Year;
            var deductionLimits = await _deductionLimitService.GetLimitValuesForYearAsync(payrollYear);
            var cpp1YtdBeforeCurrent = Math.Max(record.Cpp1Ytd - record.Cpp1, 0m);
            var cpp2YtdBeforeCurrent = Math.Max(record.Cpp2Ytd - record.Cpp2, 0m);
            var eiYtdBeforeCurrent = Math.Max(record.EiYtd - record.Ei, 0m);
            var canContributeCpp = EmployeePayrollEligibility.CanContributeCpp(
                employee.DateOfBirth,
                DateTime.Today);
            var cpp1 = canContributeCpp
                ? PayrollDeductionCaps.ClampCpp1(dto.Cpp1, cpp1YtdBeforeCurrent, deductionLimits)
                : 0m;
            var cpp2 = canContributeCpp
                ? PayrollDeductionCaps.ClampCpp2(dto.Cpp2, cpp2YtdBeforeCurrent, deductionLimits)
                : 0m;
            var ei = EmployeePayrollEligibility.CanContributeEi(employee.EiExempt)
                ? PayrollDeductionCaps.ClampEi(dto.Ei, eiYtdBeforeCurrent, deductionLimits)
                : 0m;
            var vacationSetting = VacationSettings.Normalize(dto.VacationSetting);
            var oldVacationUsed = VacationSettings.IsPaidOut(record.VacationSetting) ? record.Vacation : 0m;
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

            var oldGross = record.Gross;
            var oldNetPay = record.NetPay;
            var oldTax1 = record.Tax1;
            var oldCpp1 = record.Cpp1;
            var oldCpp2 = record.Cpp2;
            var oldEi = record.Ei;
            var oldOtherDeduction = record.OtherDeduction;
            var oldVacation = record.Vacation;

            record.Salary = salary;
            record.Rate = rate;
            record.RegularHours = dto.RegularHours;
            record.OvertimeHours = dto.OvertimeHours;
            record.PublicHolidayHours = dto.PublicHolidayHours;
            record.StatHolidayHours = dto.StatHolidayHours;
            record.Gross = dto.Gross;
            record.Vacation = dto.Vacation;
            record.VacationSetting = vacationSetting;
            record.Tax1 = dto.Tax1;
            record.Cpp1 = cpp1;
            record.Cpp2 = cpp2;
            record.Ei = ei;
            record.OtherDeduction = dto.OtherDeduction;
            record.NetPay = netPay;

            record.GrossYtd = ApplyDelta(record.GrossYtd, oldGross, dto.Gross);
            record.NetYtd = ApplyDelta(record.NetYtd, oldNetPay, netPay);
            record.TaxYtd = ApplyDelta(record.TaxYtd, oldTax1, dto.Tax1);
            record.Cpp1Ytd = ApplyDelta(record.Cpp1Ytd, oldCpp1, cpp1);
            record.Cpp2Ytd = ApplyDelta(record.Cpp2Ytd, oldCpp2, cpp2);
            record.EiYtd = ApplyDelta(record.EiYtd, oldEi, ei);
            record.OtherDeductionYtd = ApplyDelta(record.OtherDeductionYtd, oldOtherDeduction, dto.OtherDeduction);
            record.VacationYtd = ApplyDelta(record.VacationYtd, oldVacation, dto.Vacation);
            record.VacationYtdUsed = ApplyDelta(record.VacationYtdUsed, oldVacationUsed, vacationUsed);
            record.VacationYtdBalance = record.VacationYtd - record.VacationYtdUsed;

            var latestHistory = await _dbContext.RecordHistory
                .Where(history => history.PayrollRecordId == record.Id)
                .OrderByDescending(history => history.PayDate)
                .ThenByDescending(history => history.SavedAt)
                .ThenByDescending(history => history.Id)
                .FirstOrDefaultAsync();

            if (latestHistory != null)
            {
                ApplyLatestHistoryCorrection(latestHistory, record, dto, vacationSetting, vacationUsed, netPay);
            }

            await _dbContext.SaveChangesAsync();

            var result = new PayrollRecordEditResultDto
            {
                HistoryUpdated = latestHistory != null,
                Pd7Month = latestHistory?.PeriodEnding.Month,
                Pd7Year = latestHistory?.PeriodEnding.Year
            };

            if (latestHistory != null)
            {
                var pd7Month = latestHistory.PeriodEnding.Month;
                var pd7Year = latestHistory.PeriodEnding.Year;

                var existingPd7 = await _pd7LookupService.GetByPeriodAsync(
                    record.RpNumber,
                    pd7Month,
                    pd7Year);

                if (existingPd7 != null)
                {
                    await _pd7LookupService.SaveAutomaticPd7Async(
                        record.RpNumber,
                        pd7Month,
                        pd7Year);

                    result.Pd7Updated = true;
                }
            }

            await transaction.CommitAsync();

            return result;
        });
    }

    private static void ApplyLatestHistoryCorrection(
        HWAY.Payroll.Data.Entities.RecordHistory history,
        HWAY.Payroll.Data.Entities.PayrollRecord record,
        PayrollRecordEditDto dto,
        string vacationSetting,
        decimal vacationUsed,
        decimal netPay)
    {
        var paidVacation = VacationSettings.IsPaidOut(vacationSetting) ? dto.Vacation : 0m;
        var totalDeductions = dto.Tax1 + record.Cpp1 + record.Cpp2 + record.Ei + dto.OtherDeduction;
        var totalDeductionsYtd =
            record.TaxYtd +
            record.Cpp1Ytd +
            record.Cpp2Ytd +
            record.EiYtd +
            record.OtherDeductionYtd;

        history.Salary = record.Salary;
        history.Rate = record.Rate;
        history.RegularHours = dto.RegularHours;
        history.OvertimeHours = dto.OvertimeHours;
        history.PublicHolidayHours = dto.PublicHolidayHours;
        history.StatHolidayHours = dto.StatHolidayHours;
        history.Vacation = dto.Vacation;
        history.VacationUsed = vacationUsed;
        history.VacationSetting = vacationSetting;
        history.Gross = dto.Gross;
        history.PaidVacation = paidVacation;
        history.TotalPay = dto.Gross + paidVacation;
        history.Tax1 = dto.Tax1;
        history.Cpp1 = record.Cpp1;
        history.Cpp2 = record.Cpp2;
        history.Ei = record.Ei;
        history.OtherDeduction = dto.OtherDeduction;
        history.TotalDeductions = totalDeductions;
        history.NetPay = netPay;
        history.GrossYtd = record.GrossYtd;
        history.NetYtd = record.NetYtd;
        history.TaxYtd = record.TaxYtd;
        history.Cpp1Ytd = record.Cpp1Ytd;
        history.Cpp2Ytd = record.Cpp2Ytd;
        history.EiYtd = record.EiYtd;
        history.OtherDeductionYtd = record.OtherDeductionYtd;
        history.VacationYtd = record.VacationYtd;
        history.VacationYtdUsed = record.VacationYtdUsed;
        history.VacationYtdBalance = record.VacationYtdBalance;
        history.TotalPayYtd = record.GrossYtd + record.VacationYtdUsed;
        history.TotalDeductionsYtd = totalDeductionsYtd;
        history.TotalPd7 = record.TotalPd7;
    }

    private static decimal ApplyDelta(decimal currentYtd, decimal oldCurrent, decimal newCurrent)
    {
        return currentYtd - oldCurrent + newCurrent;
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
