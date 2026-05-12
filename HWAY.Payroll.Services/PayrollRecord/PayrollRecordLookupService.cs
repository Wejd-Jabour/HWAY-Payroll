using HWAY.Payroll.Data.Context;
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

                EmployeeAddress = record.Employee != null ? record.Employee.Address ?? "" : "",
                EmployeeCity = record.Employee != null ? record.Employee.City ?? "" : "",
                EmployeeProvince = record.Employee != null ? record.Employee.Province ?? "" : "",
                EmployeePostalCode = record.Employee != null ? record.Employee.PostalCode ?? "" : "",
                EmployeeEmailAddress = record.Employee != null ? record.Employee.EmailAddress ?? "" : "",

                Salary = record.Salary,
                Rate = record.Rate,
                RegularHours = record.RegularHours,
                OvertimeHours = record.OvertimeHours,
                PublicHolidayHours = record.PublicHolidayHours,
                StatHolidayHours = record.StatHolidayHours,
                Vacation = record.Vacation,
                Gross = record.Gross,

                Tax1 = record.Tax1,
                Cpp1 = record.Cpp1,
                Cpp2 = record.Cpp2,
                Ei = record.Ei,
                OtherDeduction = record.OtherDeduction,
                NetPay = record.NetPay,

                GrossYtd = record.GrossYtd,
                NetYtd = record.NetYtd,
                TaxYtd = record.TaxYtd,
                Cpp1Ytd = record.Cpp1Ytd,
                Cpp2Ytd = record.Cpp2Ytd,
                EiYtd = record.EiYtd,
                OtherDeductionYtd = record.OtherDeductionYtd,
                VacationYtd = record.VacationYtd,
                VacationYtdUsed = record.VacationYtdUsed,
                VacationYtdBalance = record.VacationYtdBalance,
                TotalPd7 = record.TotalPd7,
                HireDate = record.HireDate
            })
            .ToListAsync();
    }

    public async Task<List<PayrollRecordLookupDto>> SearchPayrollRecordsByRpNumberAsync(string rpNumber)
    {
        rpNumber = rpNumber.Trim();

        return await _dbContext.PayrollRecords
            .AsNoTracking()
            .Where(record => record.RpNumber == rpNumber)
            .OrderBy(record => record.EmployeeNumber)
            .Select(record => new PayrollRecordLookupDto
            {
                Id = record.Id,
                EmployeeNumber = record.EmployeeNumber,
                RpNumber = record.RpNumber,
                EmployeeName = record.EmployeeName,

                EmployeeAddress = record.Employee != null ? record.Employee.Address ?? "" : "",
                EmployeeCity = record.Employee != null ? record.Employee.City ?? "" : "",
                EmployeeProvince = record.Employee != null ? record.Employee.Province ?? "" : "",
                EmployeePostalCode = record.Employee != null ? record.Employee.PostalCode ?? "" : "",
                EmployeeEmailAddress = record.Employee != null ? record.Employee.EmailAddress ?? "" : "",

                Salary = record.Salary,
                Rate = record.Rate,
                RegularHours = record.RegularHours,
                
                OvertimeHours = record.OvertimeHours,
                PublicHolidayHours = record.PublicHolidayHours,
                StatHolidayHours = record.StatHolidayHours,
                Vacation = record.Vacation,
                Gross = record.Gross,

                Tax1 = record.Tax1,
                Cpp1 = record.Cpp1,
                Cpp2 = record.Cpp2,
                Ei = record.Ei,
                OtherDeduction = record.OtherDeduction,
                NetPay = record.NetPay,

                GrossYtd = record.GrossYtd,
                NetYtd = record.NetYtd,
                TaxYtd = record.TaxYtd,
                Cpp1Ytd = record.Cpp1Ytd,
                Cpp2Ytd = record.Cpp2Ytd,
                EiYtd = record.EiYtd,
                OtherDeductionYtd = record.OtherDeductionYtd,
                VacationYtd = record.VacationYtd,
                VacationYtdUsed = record.VacationYtdUsed,
                VacationYtdBalance = record.VacationYtdBalance,
                TotalPd7 = record.TotalPd7,
                HireDate = record.HireDate
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

                EmployeeAddress = record.Employee != null ? record.Employee.Address ?? "" : "",
                EmployeeCity = record.Employee != null ? record.Employee.City ?? "" : "",
                EmployeeProvince = record.Employee != null ? record.Employee.Province ?? "" : "",
                EmployeePostalCode = record.Employee != null ? record.Employee.PostalCode ?? "" : "",
                EmployeeEmailAddress = record.Employee != null ? record.Employee.EmailAddress ?? "" : "",

                Salary = record.Salary,
                Rate = record.Rate,
                RegularHours = record.RegularHours,
                OvertimeHours = record.OvertimeHours,
                PublicHolidayHours = record.PublicHolidayHours,
                StatHolidayHours = record.StatHolidayHours,
                Vacation = record.Vacation,
                Gross = record.Gross,

                Tax1 = record.Tax1,
                Cpp1 = record.Cpp1,
                Cpp2 = record.Cpp2,
                Ei = record.Ei,
                OtherDeduction = record.OtherDeduction,
                NetPay = record.NetPay,

                GrossYtd = record.GrossYtd,
                NetYtd = record.NetYtd,
                TaxYtd = record.TaxYtd,
                Cpp1Ytd = record.Cpp1Ytd,
                Cpp2Ytd = record.Cpp2Ytd,
                EiYtd = record.EiYtd,
                OtherDeductionYtd = record.OtherDeductionYtd,
                VacationYtd = record.VacationYtd,
                VacationYtdUsed = record.VacationYtdUsed,
                VacationYtdBalance = record.VacationYtdBalance,
                TotalPd7 = record.TotalPd7,
                HireDate = record.HireDate
            })
            .FirstOrDefaultAsync();
    }

    public async Task UpdatePayrollRecordAsync(int payrollRecordId, PayrollRecordUpdateDto dto)
    {
        var record = await _dbContext.PayrollRecords
            .Where(r => r.Id == payrollRecordId)
            .FirstOrDefaultAsync();

        if (record == null)
            throw new InvalidOperationException($"Payroll record with Id {payrollRecordId} not found.");

        var netPay = CalculateNetPay(
            dto.Gross,
            dto.Vacation,
            dto.Tax1,
            dto.Cpp1,
            dto.Cpp2,
            dto.Ei,
            dto.OtherDeduction);

        record.Salary = dto.Salary;
        record.Rate = dto.Rate;
        record.RegularHours = dto.RegularHours;
        record.HireDate = dto.HireDate;
        record.OvertimeHours = dto.OvertimeHours;
        record.PublicHolidayHours = dto.PublicHolidayHours;
        record.StatHolidayHours = dto.StatHolidayHours;
        record.Gross = dto.Gross;
        record.Vacation = dto.Vacation;
        record.Tax1 = dto.Tax1;
        record.Cpp1 = dto.Cpp1;
        record.Cpp2 = dto.Cpp2;
        record.Ei = dto.Ei;
        record.OtherDeduction = dto.OtherDeduction;
        record.NetPay = netPay;

        record.GrossYtd += dto.Gross;
        record.NetYtd += netPay;
        record.TaxYtd += dto.Tax1;
        record.Cpp1Ytd += dto.Cpp1;
        record.Cpp2Ytd += dto.Cpp2;
        record.EiYtd += dto.Ei;
        record.OtherDeductionYtd += dto.OtherDeduction;


        var updatedVacationYtd = record.VacationYtd + dto.Vacation;
        var updatedVacationYtdUsed = record.VacationYtdUsed + dto.VacationUsed;

        record.VacationYtd = updatedVacationYtd;
        record.VacationYtdUsed = updatedVacationYtdUsed;
        record.VacationYtdBalance = updatedVacationYtd - updatedVacationYtdUsed;

        await _dbContext.SaveChangesAsync();
    }

    private static decimal CalculateNetPay(
        decimal gross,
        decimal vacation,
        decimal tax1,
        decimal cpp1,
        decimal cpp2,
        decimal ei,
        decimal otherDeduction)
    {
        var totalDeductions = tax1 + cpp1 + cpp2 + ei + otherDeduction;

        return gross + vacation - totalDeductions;
    }
}
