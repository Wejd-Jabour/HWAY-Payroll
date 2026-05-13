namespace HWAY.Payroll.Services.EmployerInsert;

public interface IEmployerInsertService
{
    Task<int> CreateEmployerAsync(EmployerInsertDto dto);
}