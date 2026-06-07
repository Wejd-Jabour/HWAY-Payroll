using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Services.PayPeriods;
using DataPD7 = HWAY.Payroll.Data.Entities.PD7;
using Microsoft.EntityFrameworkCore;

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

        var ytdTotals = await _dbContext.PayrollRecords
            .AsNoTracking()
            .Where(record => record.RpNumber == rpNumber)
            .GroupBy(record => record.RpNumber)
            .Select(group => new
            {
                Gross = group.Sum(record => record.GrossYtd),
                Tax = group.Sum(record => record.TaxYtd),
                Cpp = group.Sum(record => record.Cpp1Ytd + record.Cpp2Ytd),
                Ei = group.Sum(record => record.EiYtd)
            })
            .FirstOrDefaultAsync();

        var previousTotals = await _dbContext.PD7Records
            .AsNoTracking()
            .Where(record =>
                record.RPNumber == rpNumber &&
                record.Year == year &&
                record.Month < month)
            .GroupBy(record => record.RPNumber)
            .Select(group => new
            {
                Gross = group.Sum(record => record.Gross),
                PD7Total = group.Sum(record => record.PD7Total)
            })
            .FirstOrDefaultAsync();

        var activeEmployees = await _dbContext.Employees
            .AsNoTracking()
            .CountAsync(employee => employee.RpNumber == rpNumber);

        if (activeEmployees == 0)
        {
            activeEmployees = await _dbContext.PayrollRecords
                .AsNoTracking()
                .CountAsync(record => record.RpNumber == rpNumber);
        }

        var gross = CalculateCurrentMonthAmount(
            ytdTotals?.Gross ?? 0,
            previousTotals?.Gross ?? 0);

        var pd7Ytd =
            ((ytdTotals?.Cpp ?? 0) * 2m) +
            (ytdTotals?.Tax ?? 0) +
            ((ytdTotals?.Ei ?? 0) + (1.4m * (ytdTotals?.Ei ?? 0)));

        var pd7Total = CalculateCurrentMonthAmount(
            pd7Ytd,
            previousTotals?.PD7Total ?? 0);
        var periodSummary = await _payPeriodRangeService.GetPayPeriodMonthSummaryAsync(
            employer.EmployerGroup,
            month,
            year);

        return new PD7LookupDto
        {
            RPNumber = employer.RpNumber,
            Fee = RoundMoney(employer.Fee),
            Gross = gross,
            PD7Total = pd7Total,
            ActiveEmployees = activeEmployees,
            Period = periodSummary.PeriodSummary,
            Transfer = RoundMoney(employer.Fee + pd7Total),
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

    private static decimal CalculateCurrentMonthAmount(decimal ytdAmount, decimal previousMonthsAmount)
    {
        var amount = ytdAmount - previousMonthsAmount;
        return amount < 0 ? 0 : RoundMoney(amount);
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

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
