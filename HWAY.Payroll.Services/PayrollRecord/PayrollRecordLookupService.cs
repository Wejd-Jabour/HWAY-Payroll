using HWAY.Payroll.Data.Context;
using HWAY.Payroll.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.PayrollRecord;

public class PayrollRecordLookupService : IPayrollRecordLookupService
{
    private readonly AppDbContext _dbContext;

    public PayrollRecordLookupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PayrollRecordLookupDto>> SearchPayrollRecordsAsync(string? searchText)
    {
        searchText = searchText?.Trim();

        var query = _dbContext.PayrollRecords
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(record =>
                record.EmployeeNumber.Contains(searchText) ||
                record.EmployeeName.Contains(searchText) ||
                record.RpNumber.Contains(searchText));
        }

        return await query
            .OrderBy(record => record.EmployeeNumber)
            .Take(25)
            .Select(record => new PayrollRecordLookupDto
            {
                Id = record.Id,
                EmployeeNumber = record.EmployeeNumber,
                RpNumber = record.RpNumber,
                EmployeeName = record.EmployeeName,

                Salary = record.Salary,
                Rate = record.Rate,
                RegularHours = record.RegularHours,
                OvertimeHours = record.OvertimeHours,
                PublicHolidayHours = record.PublicHolidayHours,
                StatHolidayHours = record.StatHolidayHours,
                Vacation = record.Vacation,
                Gross = record.Gross,

                NetPay = record.NetPay
            })
            .ToListAsync();
    }

    public async Task<PayrollRecordLookupDto?> GetPayrollRecordByEmployeeNumberAsync(string employeeNumber)
    {
        employeeNumber = employeeNumber.Trim();

        return await _dbContext.PayrollRecords
            .AsNoTracking()
            .Where(record => record.EmployeeNumber == employeeNumber)
            .Select(record => new PayrollRecordLookupDto
            {
                Id = record.Id,
                EmployeeNumber = record.EmployeeNumber,
                RpNumber = record.RpNumber,
                EmployeeName = record.EmployeeName,

                Salary = record.Salary,
                Rate = record.Rate,
                RegularHours = record.RegularHours,
                OvertimeHours = record.OvertimeHours,
                PublicHolidayHours = record.PublicHolidayHours,
                StatHolidayHours = record.StatHolidayHours,
                Vacation = record.Vacation,
                Gross = record.Gross,

                NetPay = record.NetPay
            })
            .FirstOrDefaultAsync();
    }
}