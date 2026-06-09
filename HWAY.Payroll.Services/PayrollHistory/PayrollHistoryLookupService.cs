using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Data.Entities;
using HWAY.Payroll.Services.PayPeriods;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.PayrollHistory;

public sealed class PayrollHistoryLookupService : IPayrollHistoryLookupService
{
    private readonly AppDbContext _dbContext;

    public PayrollHistoryLookupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PayrollHistoryTermOptionDto>> GetTermOptionsAsync(string rpNumber)
    {
        rpNumber = NormalizeRequiredRpNumber(rpNumber);

        var groups = await _dbContext.RecordHistory
            .AsNoTracking()
            .Where(record => record.RpNumber == rpNumber)
            .Select(record => record.EmployerGroup)
            .Distinct()
            .ToListAsync();

        return groups
            .Select(PayPeriodRules.NormalizeEmployerGroup)
            .Distinct()
            .OrderBy(group => GetGroupSortOrder(group))
            .Select(group => new PayrollHistoryTermOptionDto { Value = group })
            .ToList();
    }

    public async Task<List<int>> GetYearsAsync(string rpNumber, string? employerGroup)
    {
        var query = BuildBaseQuery(rpNumber, employerGroup);

        return await query
            .Select(record => record.Year)
            .Distinct()
            .OrderByDescending(year => year)
            .ToListAsync();
    }

    public async Task<List<PayrollHistoryMonthOptionDto>> GetMonthsAsync(
        string rpNumber,
        string? employerGroup,
        int year)
    {
        var query = BuildBaseQuery(rpNumber, employerGroup)
            .Where(record => record.Year == year);

        var months = await query
            .Select(record => record.Month)
            .Distinct()
            .OrderBy(month => month)
            .ToListAsync();

        return months
            .Select(month => new PayrollHistoryMonthOptionDto { Number = month })
            .ToList();
    }

    public async Task<List<int>> GetPeriodsAsync(
        string rpNumber,
        string? employerGroup,
        int year,
        int? month)
    {
        var query = BuildBaseQuery(rpNumber, employerGroup)
            .Where(record => record.Year == year);

        if (month.HasValue)
        {
            query = query.Where(record => record.Month == month.Value);
        }

        return await query
            .Select(record => record.Period)
            .Distinct()
            .OrderBy(period => period)
            .ToListAsync();
    }

    public async Task<PayrollHistorySearchResultDto> SearchAsync(PayrollHistorySearchRequest request)
    {
        var query = BuildBaseQuery(request.RpNumber, request.EmployerGroup);

        if (request.Year.HasValue)
        {
            query = query.Where(record => record.Year == request.Year.Value);
        }

        if (request.Month.HasValue)
        {
            query = query.Where(record => record.Month == request.Month.Value);
        }

        if (request.Period.HasValue)
        {
            query = query.Where(record => record.Period == request.Period.Value);
        }

        var employeeSearchText = request.EmployeeSearchText?.Trim();
        if (!string.IsNullOrWhiteSpace(employeeSearchText))
        {
            var normalizedSearchText = employeeSearchText.ToLowerInvariant();

            query = query.Where(record =>
                record.EmployeeNumber.ToLower().Contains(normalizedSearchText) ||
                record.EmployeeName.ToLower().Contains(normalizedSearchText));
        }

        var records = await query
            .OrderBy(record => record.PayDate)
            .ThenBy(record => record.Period)
            .ThenBy(record => record.EmployeeName)
            .ThenBy(record => record.EmployeeNumber)
            .Select(record => new PayrollHistoryRecordDto
            {
                Id = record.Id,
                PayrollRecordId = record.PayrollRecordId,
                EmployeeNumber = record.EmployeeNumber,
                EmployeeName = record.EmployeeName,
                RpNumber = record.RpNumber,
                EmployerName = record.EmployerName,
                EmployerGroup = record.EmployerGroup,
                EmployeeAddress = record.EmployeeAddress,
                EmployeeCity = record.EmployeeCity,
                EmployeeProvince = record.EmployeeProvince,
                EmployeePostalCode = record.EmployeePostalCode,
                EmployeeEmailAddress = record.EmployeeEmailAddress,
                Period = record.Period,
                PeriodBeginning = record.PeriodBeginning,
                PeriodEnding = record.PeriodEnding,
                PayDate = record.PayDate,
                Month = record.Month,
                Year = record.Year,
                SavedAt = record.SavedAt,
                Salary = record.Salary,
                Rate = record.Rate,
                RegularHours = record.RegularHours,
                OvertimeHours = record.OvertimeHours,
                PublicHolidayHours = record.PublicHolidayHours,
                StatHolidayHours = record.StatHolidayHours,
                Vacation = record.Vacation,
                VacationUsed = record.VacationUsed,
                VacationSetting = record.VacationSetting,
                Gross = record.Gross,
                PaidVacation = record.PaidVacation,
                TotalPay = record.TotalPay,
                Tax1 = record.Tax1,
                Cpp1 = record.Cpp1,
                Cpp2 = record.Cpp2,
                Ei = record.Ei,
                OtherDeduction = record.OtherDeduction,
                TotalDeductions = record.TotalDeductions,
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
                TotalPayYtd = record.TotalPayYtd,
                TotalDeductionsYtd = record.TotalDeductionsYtd,
                TotalPd7 = record.TotalPd7
            })
            .ToListAsync();

        return new PayrollHistorySearchResultDto
        {
            Records = records,
            RecordCount = records.Count,
            EmployeeCount = records
                .Select(record => record.EmployeeNumber)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count(),
            Gross = RoundMoney(records.Sum(record => record.Gross)),
            TotalPay = RoundMoney(records.Sum(record => record.TotalPay)),
            TotalDeductions = RoundMoney(records.Sum(record => record.TotalDeductions)),
            NetPay = RoundMoney(records.Sum(record => record.NetPay))
        };
    }

    private IQueryable<RecordHistory> BuildBaseQuery(string rpNumber, string? employerGroup)
    {
        rpNumber = NormalizeRequiredRpNumber(rpNumber);

        var query = _dbContext.RecordHistory
            .AsNoTracking()
            .Where(record => record.RpNumber == rpNumber);

        return ApplyEmployerGroupFilter(query, employerGroup);
    }

    private static IQueryable<RecordHistory> ApplyEmployerGroupFilter(
        IQueryable<RecordHistory> query,
        string? employerGroup)
    {
        if (string.IsNullOrWhiteSpace(employerGroup))
        {
            return query;
        }

        var normalizedGroup = PayPeriodRules.NormalizeEmployerGroup(employerGroup);

        return normalizedGroup == "A"
            ? query.Where(record => record.EmployerGroup == "A" || record.EmployerGroup == "")
            : query.Where(record => record.EmployerGroup == normalizedGroup);
    }

    private static string NormalizeRequiredRpNumber(string rpNumber)
    {
        rpNumber = rpNumber.Trim();

        if (string.IsNullOrWhiteSpace(rpNumber))
        {
            throw new InvalidOperationException("RP Number is required.");
        }

        return rpNumber;
    }

    private static int GetGroupSortOrder(string employerGroup)
    {
        return employerGroup switch
        {
            "A" => 1,
            "B" => 2,
            "C" => 3,
            "D" => 4,
            _ => 5
        };
    }

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
