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
            query = query.Where(p => p.RPNumber.Contains(searchText) || p.Period.Contains(searchText));
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

    public async Task<bool> CreatePd7Async(PD7LookupDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RPNumber))
            throw new Exception("RP Number is required.");

        if (string.IsNullOrWhiteSpace(dto.Period))
            throw new Exception("Period is required.");

        var exists = await _dbContext.PD7Records.AnyAsync(x => x.RPNumber == dto.RPNumber && x.Period == dto.Period);
        if (exists)
            throw new Exception("A PD7 record for this RP number and period already exists.");

        var entity = new DataPD7
        {
            RPNumber = dto.RPNumber,
            Fee = dto.Fee,
            Gross = dto.Gross,
            PD7Total = dto.PD7Total,
            ActiveEmployees = dto.ActiveEmployees,
            Period = dto.Period,
            Transfer = dto.Transfer,
            Received = dto.Received,
            FeeCheck = dto.FeeCheck,
            Month = dto.Month,
            Year = dto.Year
        };

        _dbContext.PD7Records.Add(entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
