namespace HWAY.Payroll.Services.EmployeeUpdate;

public interface IEmployeeUpdateService
{
    Task<List<EmployeeLookupDto>> SearchEmployeesAsync(string? searchText);
    Task<List<EmployeeLookupDto>> SearchEmployeesAsync(string? searchText, string? rpNumber);
    Task<EmployeeLookupDto?> GetEmployeeByRpNumberAsync(string rpNumber);
    Task<int> UpdateEmployeeAsync(EmployeeUpdateDto dto);
}
