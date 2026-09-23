using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Data.Entities;
using HWAY.Payroll.Domain;
using HWAY.Payroll.Services.EmployeeInsert;
using HWAY.Payroll.Services.PayPeriods;
using Microsoft.EntityFrameworkCore;
using DataPayrollRecord = HWAY.Payroll.Data.Entities.PayrollRecord;

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
        var rpNumber = dto.RpNumber.Trim();
        var employeeNumber = dto.EmployeeNumber.Trim();
        var employer = await _dbContext.Employers
            .FirstOrDefaultAsync(e => e.RpNumber == rpNumber);

        if (employer == null)
            throw new Exception("Employer not found for this RP number");

        var exists = await _dbContext.Employees
            .AnyAsync(e =>
                e.RpNumber == rpNumber &&
                e.EmployeeNumber == employeeNumber);

        if (exists)
            throw new Exception("Employee number already exists for this RP number");

        var employeeName = BuildEmployeeName(dto.FirstName, dto.LastName);
        
        var employee = new Employee
        {
            RpNumber = rpNumber,
            EmployeeNumber = employeeNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Sin = dto.Sin,
            Address = dto.Address,
            City = dto.City,
            Province = dto.Province,
            PostalCode = dto.PostalCode,
            EmailAddress = dto.EmailAddress,
            HireDate = dto.HireDate?.Date,
            DateOfBirth = dto.DateOfBirth?.Date,
            EiExempt = dto.EiExempt
        };

        var payrollRecord = new DataPayrollRecord
        {
            RpNumber = rpNumber,
            EmployeeNumber = employeeNumber,
            EmployeeName = employeeName,
            Period = PayPeriodRules.NormalizePeriod(employer.EmployerGroup, employer.PaystubPeriod),
            VacationSetting = VacationSettings.Accumulate,
            Employee = employee,
            Employer = employer
        };

        _dbContext.Employees.Add(employee);
        _dbContext.PayrollRecords.Add(payrollRecord);
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
