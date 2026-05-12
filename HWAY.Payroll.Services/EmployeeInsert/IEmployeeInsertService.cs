namespace HWAY.Payroll.Services.EmployeeInsert;

public interface IEmployeeInsertService
{
    Task<int> CreateEmployeeAsync(EmployeeInsertDto dto);
}