using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Services.PayPeriods;
using DataPD7 = HWAY.Payroll.Data.Entities.PD7;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;
using System.Globalization;
using System.Text;

namespace HWAY.Payroll.Services.PD7;

public class PD7LookupService : IPD7LookupService
{
    private readonly AppDbContext _dbContext;
    private readonly IPayPeriodRangeService _payPeriodRangeService;

    public PD7LookupService(
        AppDbContext dbContext,
        IPayPeriodRangeService payPeriodRangeService)
    {
        _dbContext = dbContext;
        _payPeriodRangeService = payPeriodRangeService;
    }

    public async Task<List<PD7LookupDto>> SearchPd7Async(string? searchText)
    {
        searchText = searchText?.Trim();

        var query = _dbContext.PD7Records.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearchText = searchText.ToLowerInvariant();

            query = query.Where(p =>
                p.RPNumber.ToLower().Contains(normalizedSearchText) ||
                p.Period.ToLower().Contains(normalizedSearchText));
        }

        return await query
            .OrderBy(p => p.RPNumber)
            .ThenBy(p => p.Year)
            .ThenBy(p => p.Month)
            .Select(p => new PD7LookupDto
            {
                RPNumber = p.RPNumber,
                Fee = p.Fee,
                Gross = p.Gross,
                PD7Total = p.PD7Total,
                ActiveEmployees = p.ActiveEmployees,
                Period = p.Period,
                Transfer = p.Transfer,
                Received = p.Received,
                FeeCheck = p.FeeCheck,
                Month = p.Month,
                Year = p.Year
            })
            .ToListAsync();
    }

    public async Task<PD7LookupDto?> GetByRpNumberAsync(string rpNumber)
    {
        var p = await _dbContext.PD7Records.AsNoTracking()
            .FirstOrDefaultAsync(x => x.RPNumber == rpNumber);

        return p == null ? null : MapToDto(p);
    }

    public async Task<PD7LookupDto?> GetByPeriodAsync(string rpNumber, int month, int year)
    {
        rpNumber = rpNumber.Trim();

        if (string.IsNullOrWhiteSpace(rpNumber))
            throw new Exception("RP Number is required.");

        if (month < 1 || month > 12)
            throw new Exception("Month must be between 1 and 12.");

        if (year < 1)
            throw new Exception("Year is required.");

        var p = await _dbContext.PD7Records
            .AsNoTracking()
            .FirstOrDefaultAsync(record =>
                record.RPNumber == rpNumber &&
                record.Month == month &&
                record.Year == year);

        return p == null ? null : MapToDto(p);
    }

    public async Task<decimal> GetCalendarYearPd7TotalAsync(string rpNumber, int year)
    {
        rpNumber = rpNumber.Trim();

        if (string.IsNullOrWhiteSpace(rpNumber))
            throw new Exception("RP Number is required.");

        if (year < 1)
            throw new Exception("Year is required.");

        var total = await _dbContext.PD7Records
            .AsNoTracking()
            .Where(record => record.RPNumber == rpNumber && record.Year == year)
            .SumAsync(record => (decimal?)record.PD7Total);

        return RoundMoney(total ?? 0);
    }

    public async Task<PD7LookupDto> CalculatePd7Async(string rpNumber, int month, int year)
    {
        rpNumber = rpNumber.Trim();

        if (string.IsNullOrWhiteSpace(rpNumber))
            throw new Exception("RP Number is required.");

        if (month < 1 || month > 12)
            throw new Exception("Month must be between 1 and 12.");

        if (year < 1)
            throw new Exception("Year is required.");

        var employer = await _dbContext.Employers
            .AsNoTracking()
            .Where(e => e.RpNumber == rpNumber)
            .Select(e => new
            {
                e.RpNumber,
                e.Fee,
                e.EmployerGroup
            })
            .FirstOrDefaultAsync();

        if (employer == null)
            throw new Exception("Employer not found.");

        return await CalculatePd7ForEmployerAsync(
            employer.RpNumber,
            employer.EmployerGroup,
            employer.Fee,
            month,
            year);
    }

    public async Task<string> ExportMonthlyPd7CsvAsync(int month, int year)
    {
        if (month < 1 || month > 12)
            throw new Exception("Month must be between 1 and 12.");

        if (year < 1)
            throw new Exception("Year is required.");

        var employers = await _dbContext.Employers
            .AsNoTracking()
            .OrderBy(employer => employer.RpNumber)
            .Select(employer => new
            {
                employer.RpNumber,
                employer.EmployerName,
                employer.EmployerGroup,
                employer.Fee
            })
            .ToListAsync();

        if (employers.Count == 0)
            throw new Exception("There are no companies on file.");

        var rpNumbers = employers
            .Select(employer => employer.RpNumber)
            .ToArray();

        var savedByRpNumber = await _dbContext.PD7Records
            .AsNoTracking()
            .Where(record =>
                record.Month == month &&
                record.Year == year &&
                rpNumbers.Contains(record.RPNumber))
            .ToDictionaryAsync(record => record.RPNumber);

        var yearTotalsByRpNumber = await _dbContext.PD7Records
            .AsNoTracking()
            .Where(record =>
                record.Year == year &&
                rpNumbers.Contains(record.RPNumber))
            .GroupBy(record => record.RPNumber)
            .Select(group => new
            {
                RPNumber = group.Key,
                PD7Total = group.Sum(record => record.PD7Total)
            })
            .ToDictionaryAsync(
                total => total.RPNumber,
                total => RoundMoney(total.PD7Total));

        var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        var csv = new StringBuilder();

        AppendCsvRow(csv,
            "RP Number",
            "Employer Name",
            "Fee",
            $"PD7 Total ({year})",
            "Year",
            "Month",
            "Periods",
            "Gross",
            "Net Total",
            "Employer Contributions",
            "Active Employees",
            "PD7 Total For Month",
            "Transfer",
            "Received",
            "Calculation Check",
            "Transfer Check");

        foreach (var employer in employers)
        {
            var calculated = await CalculatePd7ForEmployerAsync(
                employer.RpNumber,
                employer.EmployerGroup,
                employer.Fee,
                month,
                year);

            savedByRpNumber.TryGetValue(employer.RpNumber, out var saved);
            yearTotalsByRpNumber.TryGetValue(employer.RpNumber, out var yearTotal);

            var received = RoundMoney(saved?.Received ?? 0);
            var calculationCheck = RoundMoney(calculated.NetTotal + calculated.PD7Total) ==
                RoundMoney(calculated.Gross + calculated.EmployerContribution);
            var transferCheck = (calculated.Transfer != 0 || received != 0) &&
                RoundMoney(received) == RoundMoney(calculated.Transfer);

            AppendCsvRow(csv,
                employer.RpNumber,
                employer.EmployerName,
                calculated.Fee,
                yearTotal,
                year,
                $"{month:00} - {monthName}",
                calculated.Period,
                calculated.Gross,
                calculated.NetTotal,
                calculated.EmployerContribution,
                calculated.ActiveEmployees,
                calculated.PD7Total,
                calculated.Transfer,
                received,
                calculationCheck ? "Yes" : "No",
                transferCheck ? "Yes" : "No");
        }

        var fileName = $"PD7_{year}_{month:00}_{MakeSafeFileName(monthName)}.csv";
        var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        await File.WriteAllTextAsync(filePath, csv.ToString(), new UTF8Encoding(true));

        return filePath;
    }

    private async Task<PD7LookupDto> CalculatePd7ForEmployerAsync(
        string rpNumber,
        string? employerGroup,
        decimal fee,
        int month,
        int year)
    {
        var periodSummary = await _payPeriodRangeService.GetPayPeriodMonthSummaryAsync(
            employerGroup,
            month,
            year);
        var periodNumbers = periodSummary.Periods
            .Select(period => period.Period)
            .ToArray();
        var periodTotals = await GetRecordHistoryTotalsAsync(rpNumber, periodNumbers, month, year);

        var gross = 0m;
        var netTotal = 0m;
        var employerContribution = 0m;
        var pd7Total = 0m;
        var activeEmployees = 0;

        if (periodTotals != null)
        {
            gross = RoundMoney(periodTotals.Gross);
            netTotal = RoundMoney(periodTotals.NetTotal);
            employerContribution = CalculateEmployerContribution(periodTotals.Cpp, periodTotals.Ei);
            pd7Total = CalculatePd7Total(periodTotals.Tax, periodTotals.Cpp, periodTotals.Ei);
            activeEmployees = periodTotals.ActiveEmployees;
        }

        return new PD7LookupDto
        {
            RPNumber = rpNumber,
            Fee = RoundMoney(fee),
            Gross = gross,
            NetTotal = netTotal,
            EmployerContribution = employerContribution,
            PD7Total = pd7Total,
            ActiveEmployees = activeEmployees,
            Period = periodSummary.PeriodSummary,
            Transfer = RoundMoney(fee + pd7Total),
            Month = month,
            Year = year
        };
    }

    public async Task<PD7LookupDto> SaveAutomaticPd7Async(string rpNumber, int month, int year)
    {
        var calculated = await CalculatePd7Async(rpNumber, month, year);
        var existing = await _dbContext.PD7Records.FirstOrDefaultAsync(record =>
            record.RPNumber == calculated.RPNumber &&
            record.Month == calculated.Month &&
            record.Year == calculated.Year);

        if (existing == null)
        {
            existing = new DataPD7
            {
                RPNumber = calculated.RPNumber,
                Month = calculated.Month,
                Year = calculated.Year
            };

            _dbContext.PD7Records.Add(existing);
        }

        existing.Fee = RoundMoney(calculated.Fee);
        existing.Gross = RoundMoney(calculated.Gross);
        existing.PD7Total = RoundMoney(calculated.PD7Total);
        existing.ActiveEmployees = calculated.ActiveEmployees;
        existing.Period = calculated.Period;
        existing.Transfer = RoundMoney(calculated.Transfer);
        existing.FeeCheck = RoundMoney(existing.Received) == RoundMoney(existing.Transfer);

        calculated.Received = existing.Received;
        calculated.FeeCheck = existing.FeeCheck;

        await _dbContext.SaveChangesAsync();

        return calculated;
    }

    public async Task<bool> CreatePd7Async(PD7LookupDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RPNumber))
            throw new Exception("RP Number is required.");

        dto.RPNumber = dto.RPNumber.Trim();

        if (string.IsNullOrWhiteSpace(dto.Period))
            throw new Exception("Period is required.");

        if (dto.Month < 1 || dto.Month > 12)
            throw new Exception("Month must be between 1 and 12.");

        if (dto.Year < 1)
            throw new Exception("Year is required.");

        var existing = await _dbContext.PD7Records.FirstOrDefaultAsync(x =>
            x.RPNumber == dto.RPNumber &&
            x.Month == dto.Month &&
            x.Year == dto.Year);

        if (existing == null)
        {
            existing = new DataPD7
            {
                RPNumber = dto.RPNumber.Trim(),
                Month = dto.Month,
                Year = dto.Year
            };

            _dbContext.PD7Records.Add(existing);
        }

        existing.Fee = RoundMoney(dto.Fee);
        existing.Gross = RoundMoney(dto.Gross);
        existing.PD7Total = RoundMoney(dto.PD7Total);
        existing.ActiveEmployees = dto.ActiveEmployees;
        existing.Period = dto.Period;
        existing.Transfer = RoundMoney(dto.Transfer);
        existing.Received = RoundMoney(dto.Received);
        existing.FeeCheck = RoundMoney(dto.Received) == RoundMoney(dto.Transfer);

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static PD7LookupDto MapToDto(DataPD7 p)
    {
        return new PD7LookupDto
        {
            RPNumber = p.RPNumber,
            Fee = p.Fee,
            Gross = p.Gross,
            PD7Total = p.PD7Total,
            ActiveEmployees = p.ActiveEmployees,
            Period = p.Period,
            Transfer = p.Transfer,
            Received = p.Received,
            FeeCheck = p.FeeCheck,
            Month = p.Month,
            Year = p.Year
        };
    }

    private async Task<PeriodPayrollTotals?> GetRecordHistoryTotalsAsync(
        string rpNumber,
        int[] periods,
        int month,
        int year)
    {
        if (periods.Length == 0)
        {
            return null;
        }

        var monthStart = new DateTime(year, month, 1);
        var nextMonthStart = monthStart.AddMonths(1);

        return await _dbContext.RecordHistory
            .AsNoTracking()
            .Where(record =>
                record.RpNumber == rpNumber &&
                periods.Contains(record.Period) &&
                record.PeriodEnding >= monthStart &&
                record.PeriodEnding < nextMonthStart)
            .GroupBy(record => record.RpNumber)
            .Select(group => new PeriodPayrollTotals
            {
                Gross = group.Sum(record => record.Gross + record.PaidVacation),
                NetTotal = group.Sum(record => record.NetPay),
                Tax = group.Sum(record => record.Tax1),
                Cpp = group.Sum(record => record.Cpp1 + record.Cpp2),
                Ei = group.Sum(record => record.Ei),
                ActiveEmployees = group.Select(record => record.EmployeeNumber).Distinct().Count()
            })
            .FirstOrDefaultAsync();
    }

    private static decimal CalculatePd7Total(decimal tax, decimal cpp, decimal ei)
    {
        return RoundMoney(tax + cpp + ei + CalculateEmployerContribution(cpp, ei));
    }

    private static decimal CalculateEmployerContribution(decimal cpp, decimal ei)
    {
        return RoundMoney(cpp + (ei * 1.4m));
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static void AppendCsvRow(StringBuilder csv, params object?[] values)
    {
        csv.AppendLine(string.Join(",", values.Select(FormatCsvValue)));
    }

    private static string FormatCsvValue(object? value)
    {
        var text = value switch
        {
            null => "",
            decimal decimalValue => decimalValue.ToString("0.00", CultureInfo.InvariantCulture),
            int intValue => intValue.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString() ?? ""
        };

        return text.Contains('"') || text.Contains(',') || text.Contains('\n') || text.Contains('\r')
            ? $"\"{text.Replace("\"", "\"\"")}\""
            : text;
    }

    private static string MakeSafeFileName(string fileName)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(invalidChar, '_');

        return fileName;
    }

    private sealed class PeriodPayrollTotals
    {
        public decimal Gross { get; set; }
        public decimal NetTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Cpp { get; set; }
        public decimal Ei { get; set; }
        public int ActiveEmployees { get; set; }
    }
}
