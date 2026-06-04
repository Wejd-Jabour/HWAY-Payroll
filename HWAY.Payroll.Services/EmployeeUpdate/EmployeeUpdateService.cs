using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Data.Entities;
using HWAY.Payroll.Services.EmployeeInsert;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.EmployeeUpdate;

public class EmployeeUpdateService : IEmployeeUpdateService
{
    private readonly AppDbContext _dbContext;

    public EmployeeUpdateService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<EmployeeLookupDto>> SearchEmployeesAsync(string? searchText)
    {
        searchText = searchText?.Trim();

        var query = _dbContext.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearchText = searchText.ToLowerInvariant();

            query = query.Where(employee =>
                employee.RpNumber.ToLower().Contains(normalizedSearchText) ||
                employee.EmployeeNumber.ToLower().Contains(normalizedSearchText) ||
                employee.FirstName.ToLower().Contains(normalizedSearchText) ||
                employee.LastName.ToLower().Contains(normalizedSearchText) ||
                (employee.FirstName + " " + employee.LastName).ToLower().Contains(normalizedSearchText));
        }

        return await query
            .OrderBy(employee => employee.RpNumber)
            .ThenBy(employee => employee.EmployeeNumber)
            .Take(25)
            .Select(employee => new EmployeeLookupDto
            {
                RpNumber = employee.RpNumber,
                EmployeeNumber = employee.EmployeeNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Sin = employee.Sin,
                Address = employee.Address,
                City = employee.City,
                Province = employee.Province,
                PostalCode = employee.PostalCode,
                EmailAddress = employee.EmailAddress
            })
            .ToListAsync();
    }

    public async Task<EmployeeLookupDto?> GetEmployeeByRpNumberAsync(string rpNumber)
    {
        rpNumber = rpNumber.Trim();

        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.RpNumber == rpNumber)
            .Select(employee => new EmployeeLookupDto
            {
                RpNumber = employee.RpNumber,
                EmployeeNumber = employee.EmployeeNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                IsActive = employee.isActive ?? true,
                Sin = employee.Sin,
                Address = employee.Address,
                City = employee.City,
                Province = employee.Province,
                PostalCode = employee.PostalCode,
                EmailAddress = employee.EmailAddress
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> UpdateEmployeeAsync(EmployeeUpdateDto dto)
    {
        var employee = await _dbContext.Employees
            .FirstOrDefaultAsync(e =>
                e.RpNumber == dto.RpNumber &&
                e.EmployeeNumber == dto.EmployeeNumber);

        if (employee == null)
            throw new Exception("Employee not found");

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.isActive = dto.IsActive;
        employee.Sin = dto.Sin;
        employee.Address = dto.Address;
        employee.City = dto.City;
        employee.Province = dto.Province;
        employee.PostalCode = dto.PostalCode;
        employee.EmailAddress = dto.EmailAddress;

        await _dbContext.SaveChangesAsync();

        return employee.Id;
    }
}
