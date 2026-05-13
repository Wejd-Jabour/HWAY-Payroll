using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Data.Entities;
using HWAY.Payroll.Services.EmployerInsert;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.EmployerInsert;

public class EmployerInsertService : IEmployerInsertService
{
    private readonly AppDbContext _dbContext;

    public EmployerInsertService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreateEmployerAsync(EmployerInsertDto dto)
    {
        var exists = await _dbContext.Employers
            .AnyAsync(c =>
                c.RpNumber == dto.RpNumber &&
                c.EmployerName == dto.EmployerName);

        if (exists)
            throw new Exception("Employer number already exists for this RP number");

        var company = new Employer
        {
            RpNumber = dto.RpNumber,
            EmployerName = dto.EmployerName,
            Address = dto.Address,
            City = dto.City,
            PostalCode = dto.PostalCode,
            EmployerGroup = dto.EmployerGroup
        };

        _dbContext.Employers.Add(company);
        await _dbContext.SaveChangesAsync();

        return company.Id;
    }
}