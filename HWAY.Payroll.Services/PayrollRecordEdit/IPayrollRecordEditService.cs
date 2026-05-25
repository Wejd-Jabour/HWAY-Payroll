namespace HWAY.Payroll.Services.PayrollRecordEdit;

public interface IPayrollRecordEditService
{
    Task UpdatePayrollRecordAsync(int payrollRecordId, PayrollRecordEditDto dto);
}