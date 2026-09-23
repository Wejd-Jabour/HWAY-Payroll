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

    public Task<List<EmployeeLookupDto>> SearchEmployeesAsync(string? searchText)
        => SearchEmployeesAsync(searchText, null);

    public async Task<List<EmployeeLookupDto>> SearchEmployeesAsync(string? searchText, string? rpNumber)
    {
        searchText = searchText?.Trim();
        rpNumber = rpNumber?.Trim();

        var query = _dbContext.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(rpNumber))
        {
            query = query.Where(employee => employee.RpNumber == rpNumber);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearchText = searchText.ToLowerInvariant();

            query = query.Where(employee =>
                (string.IsNullOrWhiteSpace(rpNumber) && employee.RpNumber.ToLower().Contains(normalizedSearchText)) ||
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
                EmailAddress = employee.EmailAddress,
                HireDate = employee.HireDate,
                DateOfBirth = employee.DateOfBirth,
                EiExempt = employee.EiExempt
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
                Sin = employee.Sin,
                Address = employee.Address,
                City = employee.City,
                Province = employee.Province,
                PostalCode = employee.PostalCode,
                EmailAddress = employee.EmailAddress,
                HireDate = employee.HireDate,
                DateOfBirth = employee.DateOfBirth,
                EiExempt = employee.EiExempt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> UpdateEmployeeAsync(EmployeeUpdateDto dto)
    {
        var rpNumber = dto.RpNumber.Trim();
        var employeeNumber = dto.EmployeeNumber.Trim();
        var employee = await _dbContext.Employees
            .FirstOrDefaultAsync(e =>
                e.RpNumber == rpNumber &&
                e.EmployeeNumber == employeeNumber);

        if (employee == null)
            throw new Exception("Employee not found");

        var employeeName = BuildEmployeeName(dto.FirstName, dto.LastName);
        var payrollRecords = await _dbContext.PayrollRecords
            .Where(record =>
                record.RpNumber == rpNumber &&
                record.EmployeeNumber == employeeNumber)
            .ToListAsync();

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Sin = dto.Sin;
        employee.Address = dto.Address;
        employee.City = dto.City;
        employee.Province = dto.Province;
        employee.PostalCode = dto.PostalCode;
        employee.EmailAddress = dto.EmailAddress;
        employee.HireDate = dto.HireDate?.Date;
        employee.DateOfBirth = dto.DateOfBirth?.Date;
        employee.EiExempt = dto.EiExempt;

        foreach (var payrollRecord in payrollRecords)
        {
            payrollRecord.EmployeeName = employeeName;
        }

        await _dbContext.SaveChangesAsync();

        return employee.Id;
    }

    private static string BuildEmployeeName(string firstName, string lastName)
    {
        return string.Join(" ", new[] { firstName, lastName }
            .Where(value => !string.IsNullOrWhiteSpace(value)))
            .Trim();
    }
}
