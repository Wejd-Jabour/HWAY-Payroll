namespace HWAY.Payroll.Services.EmployerRemove;

public interface IEmployerRemoveService
{
    Task<int> RemoveEmployerAsync(EmployerRemoveDto dto);
}