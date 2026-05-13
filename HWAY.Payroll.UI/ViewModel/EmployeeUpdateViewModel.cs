using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployeeUpdate;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployeeUpdateViewModel : INotifyPropertyChanged
{
    private readonly IEmployeeUpdateService _employeeUpdateService;

    public EmployeeUpdateViewModel(IEmployeeUpdateService employeeUpdateService)
    {
        _employeeUpdateService = employeeUpdateService;

        SaveCommand = new Command(async () => await SaveEmployeeAsync());
        SearchCommand = new Command(async () => await SearchEmployeesAsync());
    }

    // 🔹 Form fields (bind these to UI inputs)

    public string RpNumber { get; set; } = "";
    public string EmployeeNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Sin { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? EmailAddress { get; set; }

    // 🔹 UI state
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _message = "";
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private EmployeeLookupDto? _selectedEmployee;
    public EmployeeLookupDto? SelectedEmployee
    {
        get => _selectedEmployee;
        set
        {
            if (SetProperty(ref _selectedEmployee, value) && value != null)
            {
                LoadEmployee(value);
            }
        }
    }

    public string SearchText { get; set; } = "";

    public ObservableCollection<EmployeeLookupDto> Employees { get; } = new();

    // 🔹 Command
    public ICommand SaveCommand { get; }

    public ICommand SearchCommand { get; }

    private bool _isSuccess;
    public bool IsSuccess
    {
        get => _isSuccess;
        set => SetProperty(ref _isSuccess, value);
    }

    private bool Fail(string message)
    {
        Message = message;
        return false;
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(RpNumber))
            return Fail("RP Number is required");

        if (string.IsNullOrWhiteSpace(EmployeeNumber))
            return Fail("Employee Number is required");

        if (string.IsNullOrWhiteSpace(FirstName))
            return Fail("First Name is required");

        if (string.IsNullOrWhiteSpace(LastName))
            return Fail("Last Name is required");

        return true;
    }

    private async Task SaveEmployeeAsync()
    {
        if (IsBusy) return;

        if (!Validate())
        return;

        try
        {
            IsBusy = true;
            Message = "";

            var dto = new EmployeeUpdateDto
            {
                RpNumber = RpNumber,
                EmployeeNumber = EmployeeNumber,
                FirstName = FirstName,
                LastName = LastName,
                Sin = Sin,
                Address = Address,
                City = City,
                Province = Province,
                PostalCode = PostalCode,
                EmailAddress = EmailAddress
            };

            var id = await _employeeUpdateService.UpdateEmployeeAsync(dto);

            IsSuccess = true;
            Message = $"Employee updated successfully (ID: {id})";

            ClearForm();
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            Message = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchEmployeesAsync()
    {
        var results = await _employeeUpdateService.SearchEmployeesAsync(SearchText);

        Employees.Clear();

        foreach (var employee in results)
            Employees.Add(employee);
    }

    private void LoadEmployee(EmployeeLookupDto employee)
    {
        RpNumber = employee.RpNumber;
        EmployeeNumber = employee.EmployeeNumber;
        FirstName = employee.FirstName;
        LastName = employee.LastName;
        Sin = employee.Sin;
        Address = employee.Address;
        City = employee.City;
        Province = employee.Province;
        PostalCode = employee.PostalCode;
        EmailAddress = employee.EmailAddress;

        OnPropertyChanged(nameof(RpNumber));
        OnPropertyChanged(nameof(EmployeeNumber));
        OnPropertyChanged(nameof(FirstName));
        OnPropertyChanged(nameof(LastName));
        OnPropertyChanged(nameof(Sin));
        OnPropertyChanged(nameof(Address));
        OnPropertyChanged(nameof(City));
        OnPropertyChanged(nameof(Province));
        OnPropertyChanged(nameof(PostalCode));
        OnPropertyChanged(nameof(EmailAddress));
    }

    private void ClearForm()
    {
        RpNumber = "";
        EmployeeNumber = "";
        FirstName = "";
        LastName = "";
        Sin = null;
        Address = null;
        City = null;
        Province = null;
        PostalCode = null;
        EmailAddress = null;

        OnPropertyChanged(nameof(RpNumber));
        OnPropertyChanged(nameof(EmployeeNumber));
        OnPropertyChanged(nameof(FirstName));
        OnPropertyChanged(nameof(LastName));
        OnPropertyChanged(nameof(Sin));
        OnPropertyChanged(nameof(Address));
        OnPropertyChanged(nameof(City));
        OnPropertyChanged(nameof(Province));
        OnPropertyChanged(nameof(PostalCode));
        OnPropertyChanged(nameof(EmailAddress));
    }

    // 🔹 Property change boilerplate
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (Equals(field, value)) return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }

    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}