namespace HWAY.Payroll.Services.EmployeeUpdate;

public interface IEmployeeUpdateService
{
    Task<List<EmployeeLookupDto>> SearchEmployeesAsync(string? searchText);
    Task<EmployeeLookupDto?> GetEmployeeByRpNumberAsync(string rpNumber);
    Task<int> UpdateEmployeeAsync(EmployeeUpdateDto dto);
}