namespace HWAY.Payroll.Services.PayrollRecord;

public interface IPayrollRecordLookupService
{
    Task<List<PayrollRecordLookupDto>> SearchPayrollRecordsAsync(string? searchText);
    Task<List<PayrollRecordLookupDto>> SearchPayrollRecordsByRpNumberAsync(string rpNumber);
    Task<PayrollRecordLookupDto?> GetPayrollRecordByEmployeeNumberAsync(string employeeNumber);
    Task UpdatePayrollRecordAsync(int payrollRecordId, PayrollRecordUpdateDto dto);
}