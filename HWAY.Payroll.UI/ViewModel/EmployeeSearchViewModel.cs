using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployeeUpdate;
using HWAY.Payroll.Services.EmployeeInsert;
using Microsoft.Maui.Controls;
using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployeeSearchViewModel : INotifyPropertyChanged
{
    private readonly IEmployeeUpdateService _employeeUpdateService;

    public EmployeeSearchViewModel(IEmployeeUpdateService employeeUpdateService)
    {
        _employeeUpdateService = employeeUpdateService;

        Employees = new ObservableCollection<EmployeeLookupDto>();

        SearchCommand = new Command(async () => await SearchEmployeesAsync());
        SelectEmployeeCommand = new Command<EmployeeLookupDto>(async (emp) => await SelectEmployeeAsync(emp));
    }

    // 🔹 Search input
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    // 🔹 List
    public ObservableCollection<EmployeeLookupDto> Employees { get; }

    // 🔹 Commands
    public ICommand SearchCommand { get; }
    public ICommand SelectEmployeeCommand { get; }

    private async Task SearchEmployeesAsync()
    {
        var results = await _employeeUpdateService.SearchEmployeesAsync(SearchText);

        Employees.Clear();

        foreach (var emp in results)
            Employees.Add(emp);
    }

    private async Task SelectEmployeeAsync(EmployeeLookupDto employee)
    {
        if (employee == null)
            return;

        // Navigate to update page WITH selected employee
        await Shell.Current.GoToAsync(nameof(EmployeeUpdatePage),
            new Dictionary<string, object>
            {
                ["Employee"] = employee
            });
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