using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.T4;

public class T4LookupService : IT4LookupService
{
    private readonly AppDbContext _dbContext;

    public T4LookupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T4CompanyDto> CalculateCompanyT4Async(string rpNumber, int year)
    {
        rpNumber = rpNumber.Trim();

        if (string.IsNullOrWhiteSpace(rpNumber))
            throw new InvalidOperationException("RP Number is required.");

        if (year < 1)
            throw new InvalidOperationException("Year is required.");

        var rules = T4TaxYearRuleProvider.GetRules(year);
        if (rules == null)
        {
            var supportedYears = string.Join(", ", T4TaxYearRuleProvider.SupportedYears);
            throw new InvalidOperationException($"T4 tax rules are configured for {supportedYears}.");
        }

        var employer = await _dbContext.Employers
            .AsNoTracking()
            .Where(e => e.RpNumber == rpNumber)
            .Select(e => new
            {
                e.RpNumber,
                e.EmployerName,
                e.Address,
                e.City,
                e.PostalCode
            })
            .FirstOrDefaultAsync();

        if (employer == null)
            throw new InvalidOperationException("Employer not found.");

        var currentYear = DateTime.Today.Year;
        var slips = year == currentYear
            ? await BuildSlipsFromPayrollYtdAsync(rpNumber, year, rules)
            : [];
        var calculationSource = slips.Count > 0
            ? "Current payroll YTD"
            : string.Empty;

        if (slips.Count == 0)
        {
            var historyRows = await GetHistoryRowsAsync(rpNumber, year);
            slips = BuildSlipsFromHistory(historyRows, rules);
            calculationSource = historyRows.Count > 0
                ? "Payroll history"
                : string.Empty;
        }

        return new T4CompanyDto
        {
            Year = year,
            RpNumber = employer.RpNumber,
            EmployerName = employer.EmployerName,
            Address = employer.Address ?? string.Empty,
            City = employer.City ?? string.Empty,
            PostalCode = employer.PostalCode ?? string.Empty,
            CalculationSource = calculationSource,
            TaxYearRules = rules,
            Slips = slips,
            Box14Total = RoundMoney(slips.Sum(s => s.Box14EmploymentIncome)),
            Box24Total = RoundMoney(slips.Sum(s => s.Box24EiInsurableEarnings)),
            Box26Total = RoundMoney(slips.Sum(s => s.Box26CppQppPensionableEarnings)),
            Box22Total = RoundMoney(slips.Sum(s => s.Box22IncomeTaxDeducted)),
            Box16Total = RoundMoney(slips.Sum(s => s.Box16CppContributions)),
            Box16ATotal = RoundMoney(slips.Sum(s => s.Box16ACpp2Contributions)),
            Box18Total = RoundMoney(slips.Sum(s => s.Box18EiPremiums))
        };
    }

    private async Task<List<T4HistoryRow>> GetHistoryRowsAsync(string rpNumber, int year)
    {
        return await (
            from history in _dbContext.RecordHistory.AsNoTracking()
            where history.RpNumber == rpNumber && history.Year == year
            join employee in _dbContext.Employees.AsNoTracking()
                on history.EmployeeNumber equals employee.EmployeeNumber into employees
            from employee in employees.DefaultIfEmpty()
            select new T4HistoryRow
            {
                EmployeeNumber = history.EmployeeNumber,
                EmployeeName = history.EmployeeName,
                Sin = employee == null ? string.Empty : employee.Sin ?? string.Empty,
                Address = employee == null ? history.EmployeeAddress : employee.Address ?? history.EmployeeAddress,
                City = employee == null ? history.EmployeeCity : employee.City ?? history.EmployeeCity,
                Province = employee == null ? history.EmployeeProvince : employee.Province ?? history.EmployeeProvince,
                PostalCode = employee == null ? history.EmployeePostalCode : employee.PostalCode ?? history.EmployeePostalCode,
                DateOfBirth = employee == null ? null : employee.DateOfBirth,
                EiExempt = employee != null && employee.EiExempt,
                PayDate = history.PayDate,
                Gross = history.Gross,
                PaidVacation = history.PaidVacation,
                TotalPay = history.TotalPay,
                Tax1 = history.Tax1,
                Cpp1 = history.Cpp1,
                Cpp2 = history.Cpp2,
                Ei = history.Ei
            })
            .ToListAsync();
    }

    private static List<T4EmployeeSlipDto> BuildSlipsFromHistory(
        IReadOnlyCollection<T4HistoryRow> rows,
        T4TaxYearRules rules)
    {
        return rows
            .GroupBy(row => row.EmployeeNumber)
            .Select(group =>
            {
                var first = group
                    .OrderBy(row => row.PayDate)
                    .First();
                var employmentIncome = RoundMoney(group.Sum(GetEmploymentIncome));
                var cpp1 = Cap(group.Sum(row => row.Cpp1), rules.Cpp1MaximumContribution);
                var cpp2 = Cap(group.Sum(row => row.Cpp2), rules.Cpp2MaximumContribution);
                var ei = Cap(group.Sum(row => row.Ei), rules.EiMaximumPremium);
                var eiInsurableIncome = group
                    .Where(row => !row.EiExempt || row.Ei > 0)
                    .Sum(GetEmploymentIncome);
                var pensionableIncome = group
                    .Where(row =>
                        EmployeePayrollEligibility.CanContributeCpp(row.DateOfBirth, row.PayDate) ||
                        row.Cpp1 > 0 ||
                        row.Cpp2 > 0)
                    .Sum(GetEmploymentIncome);

                if (pensionableIncome == 0 && (cpp1 > 0 || cpp2 > 0))
                {
                    pensionableIncome = employmentIncome;
                }

                return new T4EmployeeSlipDto
                {
                    EmployeeNumber = first.EmployeeNumber,
                    EmployeeName = first.EmployeeName,
                    Sin = first.Sin,
                    Address = first.Address,
                    City = first.City,
                    Province = first.Province,
                    PostalCode = first.PostalCode,
                    Box14EmploymentIncome = employmentIncome,
                    Box24EiInsurableEarnings = Cap(eiInsurableIncome, rules.EiMaximumInsurableEarnings),
                    Box26CppQppPensionableEarnings = Cap(pensionableIncome, rules.Box26PensionableEarningsCap),
                    Box22IncomeTaxDeducted = RoundMoney(group.Sum(row => row.Tax1)),
                    Box16CppContributions = cpp1,
                    Box16ACpp2Contributions = cpp2,
                    Box18EiPremiums = ei
                };
            })
            .OrderBy(slip => slip.EmployeeName)
            .ThenBy(slip => slip.EmployeeNumber)
            .ToList();
    }

    private async Task<List<T4EmployeeSlipDto>> BuildSlipsFromPayrollYtdAsync(
        string rpNumber,
        int year,
        T4TaxYearRules rules)
    {
        var ytdRows = await (
            from record in _dbContext.PayrollRecords.AsNoTracking()
            where record.RpNumber == rpNumber
            join employee in _dbContext.Employees.AsNoTracking()
                on record.EmployeeNumber equals employee.EmployeeNumber into employees
            from employee in employees.DefaultIfEmpty()
            select new T4PayrollYtdRow
            {
                EmployeeNumber = record.EmployeeNumber,
                EmployeeName = record.EmployeeName,
                Sin = employee == null ? string.Empty : employee.Sin ?? string.Empty,
                Address = employee == null ? string.Empty : employee.Address ?? string.Empty,
                City = employee == null ? string.Empty : employee.City ?? string.Empty,
                Province = employee == null ? string.Empty : employee.Province ?? string.Empty,
                PostalCode = employee == null ? string.Empty : employee.PostalCode ?? string.Empty,
                DateOfBirth = employee == null ? null : employee.DateOfBirth,
                EiExempt = employee != null && employee.EiExempt,
                GrossYtd = record.GrossYtd,
                VacationYtdUsed = record.VacationYtdUsed,
                TaxYtd = record.TaxYtd,
                Cpp1Ytd = record.Cpp1Ytd,
                Cpp2Ytd = record.Cpp2Ytd,
                EiYtd = record.EiYtd
            })
            .ToListAsync();

        return ytdRows
            .Select(row =>
            {
                var employmentIncome = RoundMoney(row.GrossYtd + row.VacationYtdUsed);
                var cpp1 = Cap(row.Cpp1Ytd, rules.Cpp1MaximumContribution);
                var cpp2 = Cap(row.Cpp2Ytd, rules.Cpp2MaximumContribution);
                var ei = Cap(row.EiYtd, rules.EiMaximumPremium);
                var eiInsurableIncome = !row.EiExempt || ei > 0
                    ? employmentIncome
                    : 0m;
                var pensionableIncome =
                    EmployeePayrollEligibility.CanContributeCpp(row.DateOfBirth, new DateTime(year, 12, 31)) ||
                    cpp1 > 0 ||
                    cpp2 > 0
                        ? employmentIncome
                        : 0m;

                return new T4EmployeeSlipDto
                {
                    EmployeeNumber = row.EmployeeNumber,
                    EmployeeName = row.EmployeeName,
                    Sin = row.Sin,
                    Address = row.Address,
                    City = row.City,
                    Province = row.Province,
                    PostalCode = row.PostalCode,
                    Box14EmploymentIncome = employmentIncome,
                    Box24EiInsurableEarnings = Cap(eiInsurableIncome, rules.EiMaximumInsurableEarnings),
                    Box26CppQppPensionableEarnings = Cap(pensionableIncome, rules.Box26PensionableEarningsCap),
                    Box22IncomeTaxDeducted = RoundMoney(row.TaxYtd),
                    Box16CppContributions = cpp1,
                    Box16ACpp2Contributions = cpp2,
                    Box18EiPremiums = ei
                };
            })
            .OrderBy(slip => slip.EmployeeName)
            .ThenBy(slip => slip.EmployeeNumber)
            .ToList();
    }

    private static decimal GetEmploymentIncome(T4HistoryRow row)
    {
        var totalPay = row.TotalPay != 0
            ? row.TotalPay
            : row.Gross + row.PaidVacation;

        return totalPay < 0 ? 0 : totalPay;
    }

    private static decimal Cap(decimal value, decimal maximum)
    {
        if (value <= 0)
            return 0;

        return RoundMoney(Math.Min(value, maximum));
    }

    private static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private sealed class T4HistoryRow
    {
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Sin { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public bool EiExempt { get; set; }
        public DateTime PayDate { get; set; }
        public decimal Gross { get; set; }
        public decimal PaidVacation { get; set; }
        public decimal TotalPay { get; set; }
        public decimal Tax1 { get; set; }
        public decimal Cpp1 { get; set; }
        public decimal Cpp2 { get; set; }
        public decimal Ei { get; set; }
    }

    private sealed class T4PayrollYtdRow
    {
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Sin { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public bool EiExempt { get; set; }
        public decimal GrossYtd { get; set; }
        public decimal VacationYtdUsed { get; set; }
        public decimal TaxYtd { get; set; }
        public decimal Cpp1Ytd { get; set; }
        public decimal Cpp2Ytd { get; set; }
        public decimal EiYtd { get; set; }
    }
}
