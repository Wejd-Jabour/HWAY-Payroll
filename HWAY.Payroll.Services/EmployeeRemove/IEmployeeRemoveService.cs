namespace HWAY.Payroll.Services.EmployeeRemove;

public interface IEmployeeRemoveService
{
    Task<int> RemoveEmployeeAsync(EmployeeRemoveDto dto);
}