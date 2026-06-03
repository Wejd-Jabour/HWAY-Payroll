using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Data.Entities;
using HWAY.Payroll.Services.EmployerInsert;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.EmployerUpdate;

public class EmployerUpdateService : IEmployerUpdateService
{
    private readonly AppDbContext _dbContext;

    public EmployerUpdateService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<EmployerLookupDto>> SearchEmployersAsync(string? searchText)
    {
        searchText = searchText?.Trim();

        var query = _dbContext.Employers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearchText = searchText.ToLowerInvariant();

            query = query.Where(employer =>
                employer.RpNumber.ToLower().Contains(normalizedSearchText) ||
                employer.EmployerName.ToLower().Contains(normalizedSearchText));
        }

        return await query
            .OrderBy(employer => employer.RpNumber)
            .ThenBy(employer => employer.EmployerName)
            .Take(25)
            .Select(employer => new EmployerLookupDto
            {
                RpNumber = employer.RpNumber,
                EmployerName = employer.EmployerName,
                Address = employer.Address,
                City = employer.City,
                PostalCode = employer.PostalCode,
                EmployerGroup = employer.EmployerGroup
            })
            .ToListAsync();
    }

    public async Task<EmployerLookupDto?> GetEmployerByRpNumberAsync(string rpNumber)
    {
        rpNumber = rpNumber.Trim();

        return await _dbContext.Employers
            .AsNoTracking()
            .Where(employer => employer.RpNumber == rpNumber)
            .Select(employer => new EmployerLookupDto
            {
                RpNumber = employer.RpNumber,
                EmployerName = employer.EmployerName,
                Address = employer.Address,
                City = employer.City,
                PostalCode = employer.PostalCode,
                EmployerGroup = employer.EmployerGroup
            })
            .FirstOrDefaultAsync();
    }
    public async Task<int> UpdateEmployerAsync(EmployerUpdateDto dto)
    {
        var employer = await _dbContext.Employers
            .FirstOrDefaultAsync(e =>
                e.RpNumber == dto.RpNumber &&
                e.EmployerName == dto.EmployerName);

        if (employer == null)
            throw new Exception("Employer not found");

        employer.RpNumber = dto.RpNumber;
        employer.EmployerName = dto.EmployerName;
        employer.Address = dto.Address;
        employer.City = dto.City;
        employer.PostalCode = dto.PostalCode;
        employer.EmployerGroup = dto.EmployerGroup;

        await _dbContext.SaveChangesAsync();

        return employer.Id;
    }
}
