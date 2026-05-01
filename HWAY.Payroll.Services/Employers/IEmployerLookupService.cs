namespace HWAY.Payroll.Services.Employers;

public interface IEmployerLookupService
{
    Task<List<EmployerLookupDto>> SearchEmployersAsync(string? searchText);
    Task<EmployerLookupDto?> GetEmployerByRpNumberAsync(string rpNumber);
}