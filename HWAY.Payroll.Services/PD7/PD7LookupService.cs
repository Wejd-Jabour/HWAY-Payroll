using HWAY.Payroll.Data.Context;
using DataPD7 = HWAY.Payroll.Data.Entities.PD7;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.PD7;

public class PD7LookupService : IPD7LookupService
{
    private readonly AppDbContext _dbContext;

    public PD7LookupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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

        if (p == null) return null;

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
                e.Fee
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

        return new PD7LookupDto
        {
            RPNumber = employer.RpNumber,
            Fee = RoundMoney(employer.Fee),
            Gross = gross,
            PD7Total = pd7Total,
            ActiveEmployees = activeEmployees,
            Period = "XX",
            Transfer = RoundMoney(employer.Fee + pd7Total),
            Month = month,
            Year = year
        };
    }

    public async Task<bool> CreatePd7Async(PD7LookupDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RPNumber))
            throw new Exception("RP Number is required.");

        if (string.IsNullOrWhiteSpace(dto.Period))
            throw new Exception("Period is required.");

        if (dto.Month < 1 || dto.Month > 12)
            throw new Exception("Month must be between 1 and 12.");

        if (dto.Year < 1)
            throw new Exception("Year is required.");

        var exists = await _dbContext.PD7Records.AnyAsync(x =>
            x.RPNumber == dto.RPNumber &&
            x.Month == dto.Month &&
            x.Year == dto.Year);

        if (exists)
            throw new Exception("A PD7 record for this RP number, month, and year already exists.");

        var entity = new DataPD7
        {
            RPNumber = dto.RPNumber.Trim(),
            Fee = RoundMoney(dto.Fee),
            Gross = RoundMoney(dto.Gross),
            PD7Total = RoundMoney(dto.PD7Total),
            ActiveEmployees = dto.ActiveEmployees,
            Period = dto.Period,
            Transfer = RoundMoney(dto.Transfer),
            Received = RoundMoney(dto.Received),
            FeeCheck = RoundMoney(dto.Received) == RoundMoney(dto.Transfer),
            Month = dto.Month,
            Year = dto.Year
        };

        _dbContext.PD7Records.Add(entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static decimal CalculateCurrentMonthAmount(decimal ytdAmount, decimal previousMonthsAmount)
    {
        var amount = ytdAmount - previousMonthsAmount;
        return amount < 0 ? 0 : RoundMoney(amount);
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
