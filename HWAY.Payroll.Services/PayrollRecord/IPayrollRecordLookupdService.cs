namespace HWAY.Payroll.Services.PayrollRecord;

public interface IPayrollRecordLookupService
{
    Task<List<PayrollRecordLookupDto>> SearchPayrollRecordsAsync(string? searchText);
    Task<PayrollRecordLookupDto?> GetPayrollRecordByEmployeeNumberAsync(string employeeNumber);
}