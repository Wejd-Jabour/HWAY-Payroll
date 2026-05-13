using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployeeUpdate;
using HWAY.Payroll.Services.EmployeeInsert;
using HWAY.Payroll.Services.EmployeeRemove;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployeeRemoveViewModel : INotifyPropertyChanged
{
    private readonly IEmployeeRemoveService _employeeRemoveService;
    private readonly IEmployeeUpdateService _employeeUpdateService;

        public EmployeeRemoveViewModel(
        IEmployeeRemoveService employeeRemoveService,
        IEmployeeUpdateService employeeUpdateService)
    {
        _employeeRemoveService = employeeRemoveService;
        _employeeUpdateService = employeeUpdateService;

        Employees = new ObservableCollection<EmployeeLookupDto>();

        SearchCommand = new Command(async () => await SearchEmployeesAsync());
        DeleteCommand = new Command<EmployeeLookupDto>(async (emp) => await DeleteEmployeeAsync(emp));
    }

    // 🔹 Search input
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    // 🔹 List of employees
    public ObservableCollection<EmployeeLookupDto> Employees { get; }

    // 🔹 UI state
    private string _message = "";
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    // 🔹 Commands
    public ICommand SearchCommand { get; }
    public ICommand DeleteCommand { get; }

    // 🔍 Search employees
    private async Task SearchEmployeesAsync()
    {
        var results = await _employeeUpdateService.SearchEmployeesAsync(SearchText);

        Employees.Clear();

        foreach (var emp in results)
            Employees.Add(emp);
    }

    // 🗑 Delete employee
    private async Task DeleteEmployeeAsync(EmployeeLookupDto employee)
    {
        if (employee == null || IsBusy)
            return;

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Confirm Delete",
            $"Delete {employee.FirstName} {employee.LastName}?",
            "Yes",
            "No");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;

            var dto = new EmployeeRemoveDto
            {
                RpNumber = employee.RpNumber,
                EmployeeNumber = employee.EmployeeNumber
            };

            await _employeeRemoveService.RemoveEmployeeAsync(dto);

            Employees.Remove(employee);

            Message = "Employee deleted successfully.";
        }
        catch (Exception ex)
        {
            Message = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // 🔹 Property changed boilerplate
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (Equals(field, value)) return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}