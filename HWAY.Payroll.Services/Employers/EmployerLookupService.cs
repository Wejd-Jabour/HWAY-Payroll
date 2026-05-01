using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Services.Employers;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.Employers;

public class EmployerLookupService : IEmployerLookupService
{
    private readonly AppDbContext _dbContext;

    public EmployerLookupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<EmployerLookupDto>> SearchEmployersAsync(string? searchText)
    {
        searchText = searchText?.Trim();

        var query = _dbContext.Employers
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(employer =>
                employer.RpNumber.Contains(searchText) ||
                employer.EmployerName.Contains(searchText));
        }

        return await query
            .OrderBy(employer => employer.RpNumber)
            .Take(25)
            .Select(employer => new EmployerLookupDto
            {
                Id = employer.Id,
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
                Id = employer.Id,
                RpNumber = employer.RpNumber,
                EmployerName = employer.EmployerName,
                Address = employer.Address,
                City = employer.City,
                PostalCode = employer.PostalCode,
                EmployerGroup = employer.EmployerGroup
            })
            .FirstOrDefaultAsync();
    }
}