using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Data.Entities;
using HWAY.Payroll.Services.EmployeeInsert;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.EmployeeInsert;

public class EmployeeInsertService : IEmployeeInsertService
{
    private readonly AppDbContext _dbContext;

    public EmployeeInsertService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreateEmployeeAsync(EmployeeInsertDto dto)
    {
        var exists = await _dbContext.Employees
            .AnyAsync(e =>
                e.RpNumber == dto.RpNumber &&
                e.EmployeeNumber == dto.EmployeeNumber);

        if (exists)
            throw new Exception("Employee number already exists for this RP number");
        
        var employee = new Employee
        {
            RpNumber = dto.RpNumber,
            EmployeeNumber = dto.EmployeeNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Sin = dto.Sin,
            Address = dto.Address,
            City = dto.City,
            Province = dto.Province,
            PostalCode = dto.PostalCode,
            EmailAddress = dto.EmailAddress
        };

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        return employee.Id;
    }
}