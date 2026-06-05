using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Domain;
using HWAY.Payroll.Services;
using HWAY.Payroll.Services.PD7;
using HWAY.Payroll.Services.PayPeriods;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.PayrollRecord;

public class PayrollRecordLookupService : IPayrollRecordLookupService
{
    private readonly AppDbContext _dbContext;
    private readonly IPD7LookupService _pd7LookupService;
    private readonly IPayPeriodRangeService _payPeriodRangeService;

    public PayrollRecordLookupService(
        AppDbContext dbContext,
        IPD7LookupService pd7LookupService,
        IPayPeriodRangeService payPeriodRangeService)
    {
        _dbContext = dbContext;
        _pd7LookupService = pd7LookupService;
        _payPeriodRangeService = payPeriodRangeService;
    }

    public async Task<List<PayrollRecordLookupDto>> SearchPayrollRecordsAsync(string? searchText)
    {
        searchText = searchText?.Trim();

        var query = _dbContext.PayrollRecords
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearchText = searchText.ToLowerInvariant();

            query = query.Where(record =>
                record.EmployeeNumber.ToLower().Contains(normalizedSearchText) ||
                record.EmployeeName.ToLower().Contains(normalizedSearchText) ||
                record.RpNumber.ToLower().Contains(normalizedSearchText) ||
                record.Employer!.EmployerName.ToLower().Contains(normalizedSearchText) ||
                record.Employer.RpNumber.ToLower().Contains(normalizedSearchText));
        }

        return await query
            .OrderBy(record => record.EmployeeNumber)
            .Take(25)
            .Select(record => new PayrollRecordLookupDto
            {
                Id = record.Id,
                EmployeeNumber = record.EmployeeNumber,
                RpNumber = record.RpNumber,
                EmployeeName = record.EmployeeName,
                EmployerName = record.Employer!.EmployerName,
                Period = record.Period,

                EmployeeAddress = record.Employee != null ? record.Employee.Address ?? "" : "",
                EmployeeCity = record.Employee != null ? record.Employee.City ?? "" : "",
                EmployeeProvince = record.Employee != null ? record.Employee.Province ?? "" : "",
                EmployeePostalCode = record.Employee != null ? record.Employee.PostalCode ?? "" : "",
                EmployeeEmailAddress = record.Employee != null ? record.Employee.EmailAddress ?? "" : "",

                Salary = record.Salary,
                Rate = record.Rate,
                RegularHours = record.RegularHours,
                OvertimeHours = record.OvertimeHours,
                PublicHolidayHours = record.PublicHolidayHours,
                StatHolidayHours = record.StatHolidayHours,
                Vacation = record.Vacation,
                VacationSetting = record.VacationSetting,
                Gross = record.Gross,

                Tax1 = record.Tax1,
                Cpp1 = record.Cpp1,
                Cpp2 = record.Cpp2,
                Ei = record.Ei,
                OtherDeduction = record.OtherDeduction,
                NetPay = record.NetPay,

                GrossYtd = record.GrossYtd,
                NetYtd = record.NetYtd,
                TaxYtd = record.TaxYtd,
                Cpp1Ytd = record.Cpp1Ytd,
                Cpp2Ytd = record.Cpp2Ytd,
                EiYtd = record.EiYtd,
                OtherDeductionYtd = record.OtherDeductionYtd,
                VacationYtd = record.VacationYtd,
                VacationYtdUsed = record.VacationYtdUsed,
                VacationYtdBalance = record.VacationYtdBalance,
                TotalPd7 = record.TotalPd7,
                HireDate = record.Employee != null ? record.Employee.HireDate : null,
                DateOfBirth = record.Employee != null ? record.Employee.DateOfBirth : null,
                EiExempt = record.Employee != null && record.Employee.EiExempt
            })
            .ToListAsync();
    }

    public async Task<List<PayrollRecordLookupDto>> SearchPayrollRecordsByRpNumberAsync(string rpNumber)
    {
        rpNumber = rpNumber.Trim();

        return await _dbContext.PayrollRecords
            .AsNoTracking()
            .Where(record => record.RpNumber == rpNumber)
            .OrderBy(record => record.EmployeeNumber)
            .Select(record => new PayrollRecordLookupDto
            {
                Id = record.Id,
                EmployeeNumber = record.EmployeeNumber,
                RpNumber = record.RpNumber,
                EmployeeName = record.EmployeeName,
                EmployerName = record.Employer!.EmployerName,
                Period = record.Period,

                EmployeeAddress = record.Employee != null ? record.Employee.Address ?? "" : "",
                EmployeeCity = record.Employee != null ? record.Employee.City ?? "" : "",
                EmployeeProvince = record.Employee != null ? record.Employee.Province ?? "" : "",
                EmployeePostalCode = record.Employee != null ? record.Employee.PostalCode ?? "" : "",
                EmployeeEmailAddress = record.Employee != null ? record.Employee.EmailAddress ?? "" : "",

                Salary = record.Salary,
                Rate = record.Rate,
                RegularHours = record.RegularHours,
                OvertimeHours = record.OvertimeHours,
                PublicHolidayHours = record.PublicHolidayHours,
                StatHolidayHours = record.StatHolidayHours,
                Vacation = record.Vacation,
                VacationSetting = record.VacationSetting,
                Gross = record.Gross,

                Tax1 = record.Tax1,
                Cpp1 = record.Cpp1,
                Cpp2 = record.Cpp2,
                Ei = record.Ei,
                OtherDeduction = record.OtherDeduction,
                NetPay = record.NetPay,

                GrossYtd = record.GrossYtd,
                NetYtd = record.NetYtd,
                TaxYtd = record.TaxYtd,
                Cpp1Ytd = record.Cpp1Ytd,
                Cpp2Ytd = record.Cpp2Ytd,
                EiYtd = record.EiYtd,
                OtherDeductionYtd = record.OtherDeductionYtd,
                VacationYtd = record.VacationYtd,
                VacationYtdUsed = record.VacationYtdUsed,
                VacationYtdBalance = record.VacationYtdBalance,
                TotalPd7 = record.TotalPd7,
                HireDate = record.Employee != null ? record.Employee.HireDate : null,
                DateOfBirth = record.Employee != null ? record.Employee.DateOfBirth : null,
                EiExempt = record.Employee != null && record.Employee.EiExempt
            })
            .ToListAsync();
    }

    public async Task<PayrollRecordLookupDto?> GetPayrollRecordByEmployeeNumberAsync(string employeeNumber)
    {
        employeeNumber = employeeNumber.Trim();

        return await _dbContext.PayrollRecords
            .AsNoTracking()
            .Where(record => record.EmployeeNumber == employeeNumber)
            .Select(record => new PayrollRecordLookupDto
            {
                Id = record.Id,
                EmployeeNumber = record.EmployeeNumber,
                RpNumber = record.RpNumber,
                EmployeeName = record.EmployeeName,
                EmployerName = record.Employer!.EmployerName,
                Period = record.Period,

                EmployeeAddress = record.Employee != null ? record.Employee.Address ?? "" : "",
                EmployeeCity = record.Employee != null ? record.Employee.City ?? "" : "",
                EmployeeProvince = record.Employee != null ? record.Employee.Province ?? "" : "",
                EmployeePostalCode = record.Employee != null ? record.Employee.PostalCode ?? "" : "",
                EmployeeEmailAddress = record.Employee != null ? record.Employee.EmailAddress ?? "" : "",

                Salary = record.Salary,
                Rate = record.Rate,
                RegularHours = record.RegularHours,
                OvertimeHours = record.OvertimeHours,
                PublicHolidayHours = record.PublicHolidayHours,
                StatHolidayHours = record.StatHolidayHours,
                Vacation = record.Vacation,
                VacationSetting = record.VacationSetting,
                Gross = record.Gross,

                Tax1 = record.Tax1,
                Cpp1 = record.Cpp1,
                Cpp2 = record.Cpp2,
                Ei = record.Ei,
                OtherDeduction = record.OtherDeduction,
                NetPay = record.NetPay,

                GrossYtd = record.GrossYtd,
                NetYtd = record.NetYtd,
                TaxYtd = record.TaxYtd,
                Cpp1Ytd = record.Cpp1Ytd,
                Cpp2Ytd = record.Cpp2Ytd,
                EiYtd = record.EiYtd,
                OtherDeductionYtd = record.OtherDeductionYtd,
                VacationYtd = record.VacationYtd,
                VacationYtdUsed = record.VacationYtdUsed,
                VacationYtdBalance = record.VacationYtdBalance,
                TotalPd7 = record.TotalPd7,
                HireDate = record.Employee != null ? record.Employee.HireDate : null,
                DateOfBirth = record.Employee != null ? record.Employee.DateOfBirth : null,
                EiExempt = record.Employee != null && record.Employee.EiExempt
            })
            .FirstOrDefaultAsync();
    }

    public async Task UpdatePayrollRecordAsync(int payrollRecordId, PayrollRecordUpdateDto dto)
    {
        var record = await _dbContext.PayrollRecords
            .Include(r => r.Employer)
            .Include(r => r.Employee)
            .Where(r => r.Id == payrollRecordId)
            .FirstOrDefaultAsync();

        if (record == null)
            throw new InvalidOperationException($"Payroll record with Id {payrollRecordId} not found.");

        var period = PayPeriodRules.NormalizePeriod(
            record.Employer?.EmployerGroup,
            record.Employer?.PaystubPeriod ?? record.Period);

        ApplyPayrollUpdate(record, dto, period);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<EmployerPayrollSaveResult> UpdatePayrollRecordsForEmployerAsync(
        string rpNumber,
        IReadOnlyCollection<PayrollRecordBatchUpdateDto> updates)
    {
        rpNumber = rpNumber.Trim();

        if (updates.Count == 0)
        {
            throw new InvalidOperationException("At least one payroll record is required.");
        }

        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var employer = await _dbContext.Employers
                .Where(e => e.RpNumber == rpNumber)
                .FirstOrDefaultAsync();

            if (employer == null)
                throw new InvalidOperationException($"Employer with RP number {rpNumber} not found.");

            var savedPeriod = PayPeriodRules.NormalizePeriod(employer.EmployerGroup, employer.PaystubPeriod);
            var recordIds = updates.Select(update => update.PayrollRecordId).Distinct().ToList();
            var records = await _dbContext.PayrollRecords
                .Include(record => record.Employee)
                .Where(record => record.RpNumber == rpNumber && recordIds.Contains(record.Id))
                .ToDictionaryAsync(record => record.Id);

            foreach (var update in updates)
            {
                if (!records.TryGetValue(update.PayrollRecordId, out var record))
                {
                    throw new InvalidOperationException(
                        $"Payroll record with Id {update.PayrollRecordId} was not found for RP number {rpNumber}.");
                }

                ApplyPayrollUpdate(record, update.PayrollRecord, savedPeriod);
            }

            var nextPeriod = PayPeriodRules.GetNextPeriod(employer.EmployerGroup, savedPeriod);
            employer.PaystubPeriod = nextPeriod;

            await _dbContext.SaveChangesAsync();
            var pd7Result = await SaveAutomaticPd7IfClosingMonthAsync(
                employer.RpNumber,
                employer.EmployerGroup,
                savedPeriod);
            await transaction.CommitAsync();

            return new EmployerPayrollSaveResult(
                savedPeriod,
                nextPeriod,
                updates.Count,
                pd7Result?.Month,
                pd7Result?.Year,
                pd7Result?.Period);
        });
    }

    private async Task<PD7LookupDto?> SaveAutomaticPd7IfClosingMonthAsync(
        string rpNumber,
        string? employerGroup,
        int savedPeriod)
    {
        var closingMonth = await _payPeriodRangeService.GetClosingMonthForPeriodAsync(
            employerGroup,
            savedPeriod,
            DateTime.Today.Year);

        if (closingMonth == null)
        {
            return null;
        }

        return await _pd7LookupService.SaveAutomaticPd7Async(
            rpNumber,
            closingMonth.Month,
            closingMonth.Year);
    }

    private static void ApplyPayrollUpdate(
        HWAY.Payroll.Data.Entities.PayrollRecord record,
        PayrollRecordUpdateDto dto,
        int period)
    {
        record.Period = period;
        var salary = dto.Salary;
        var rate = salary > 0 ? 0 : dto.Rate;
        var eligibilityDate = (dto.PayDate ?? DateTime.Today).Date;
        var payrollYear = eligibilityDate.Year;
        var canContributeCpp = EmployeePayrollEligibility.CanContributeCpp(
            record.Employee?.DateOfBirth,
            eligibilityDate);
        var cpp1 = canContributeCpp
            ? PayrollDeductionCaps.ClampCpp1(dto.Cpp1, record.Cpp1Ytd, payrollYear)
            : 0m;
        var cpp2 = canContributeCpp
            ? PayrollDeductionCaps.ClampCpp2(dto.Cpp2, record.Cpp2Ytd, payrollYear)
            : 0m;
        var ei = EmployeePayrollEligibility.CanContributeEi(record.Employee?.EiExempt == true)
            ? PayrollDeductionCaps.ClampEi(dto.Ei, record.EiYtd, payrollYear)
            : 0m;
        var vacationSetting = VacationSettings.Normalize(dto.VacationSetting);
        var vacationUsed = VacationSettings.IsPaidOut(vacationSetting)
            ? dto.Vacation
            : dto.VacationUsed;

        var netPay = CalculateNetPay(
            dto.Gross,
            dto.Vacation,
            vacationSetting,
            dto.Tax1,
            cpp1,
            cpp2,
            ei,
            dto.OtherDeduction);

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
