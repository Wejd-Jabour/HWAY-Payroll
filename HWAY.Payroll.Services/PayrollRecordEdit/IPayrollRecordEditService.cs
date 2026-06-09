namespace HWAY.Payroll.Services.PayrollRecordEdit;

public interface IPayrollRecordEditService
{
    Task<PayrollRecordEditResultDto> UpdatePayrollRecordAsync(int payrollRecordId, PayrollRecordEditDto dto);
}
