namespace HWAY.Payroll.Services.EmployerUpdate;

public interface IEmployerUpdateService
{
    Task<List<EmployerLookupDto>> SearchEmployersAsync(string? searchText);
    Task<EmployerLookupDto?> GetEmployerByRpNumberAsync(string rpNumber);
    Task<int> UpdateEmployerAsync(EmployerUpdateDto dto);
}