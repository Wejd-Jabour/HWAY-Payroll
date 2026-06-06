using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployeeUpdate;
using HWAY.Payroll.UI.Views;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModels;

[QueryProperty(nameof(Employee), "Employee")]
public class EmployeeUpdateViewModel : INotifyPropertyChanged
{
    private readonly IEmployeeUpdateService _employeeUpdateService;

    public EmployeeUpdateViewModel(IEmployeeUpdateService employeeUpdateService)
    {
        _employeeUpdateService = employeeUpdateService;

        SaveCommand = new Command(async () => await SaveEmployeeAsync());
    }

    // 🔹 Incoming selected employee from navigation
    private EmployeeLookupDto? _employee;
    public EmployeeLookupDto? Employee
    {
        get => _employee;
        set
        {
            _employee = value;
            LoadEmployee(value);
        }
    }

    // 🔹 Editable fields
    private string _rpNumber = "";
    public string RpNumber
    {
        get => _rpNumber;
        set => SetProperty(ref _rpNumber, value);
    }

    private string _employeeNumber = "";
    public string EmployeeNumber
    {
        get => _employeeNumber;
        set => SetProperty(ref _employeeNumber, value);
    }

    private string _firstName = "";
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    private string _lastName = "";
    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    private string? _sin;
    public string? Sin
    {
        get => _sin;
        set => SetProperty(ref _sin, value);
    }

    private string? _address;
    public string? Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    private string? _city;
    public string? City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }

    private string? _province;
    public string? Province
    {
        get => _province;
        set => SetProperty(ref _province, value);
    }

    private string? _postalCode;
    public string? PostalCode
    {
        get => _postalCode;
        set => SetProperty(ref _postalCode, value);
    }

    private string? _emailAddress;
    public string? EmailAddress
    {
        get => _emailAddress;
        set => SetProperty(ref _emailAddress, value);
    }

    private bool _hasHireDate;
    public bool HasHireDate
    {
        get => _hasHireDate;
        set => SetProperty(ref _hasHireDate, value);
    }

    private DateTime _hireDateDisplay = DateTime.Today;
    public DateTime HireDateDisplay
    {
        get => _hireDateDisplay;
        set => SetProperty(ref _hireDateDisplay, value.Date);
    }

    private bool _hasDateOfBirth;
    public bool HasDateOfBirth
    {
        get => _hasDateOfBirth;
        set => SetProperty(ref _hasDateOfBirth, value);
    }

    private DateTime _dateOfBirthDisplay = DateTime.Today.AddYears(-18);
    public DateTime DateOfBirthDisplay
    {
        get => _dateOfBirthDisplay;
        set => SetProperty(ref _dateOfBirthDisplay, value.Date);
    }

    private bool _eiExempt;
    public bool EiExempt
    {
        get => _eiExempt;
        set => SetProperty(ref _eiExempt, value);
    }

    // 🔹 UI state
    private string _message = "";
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private bool _isSuccess;
    public bool IsSuccess
    {
        get => _isSuccess;
        set => SetProperty(ref _isSuccess, value);
    }

    public ICommand SaveCommand { get; }

    private async Task SaveEmployeeAsync()
    {
        try
        {
            if (HasHireDate && HireDateDisplay.Date > DateTime.Today)
            {
                IsSuccess = false;
                Message = "Hire date cannot be in the future";
                return;
            }

            if (HasDateOfBirth && DateOfBirthDisplay.Date > DateTime.Today)
            {
                IsSuccess = false;
                Message = "Birth date cannot be in the future";
                return;
            }

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
                EmailAddress = EmailAddress,
                HireDate = HasHireDate ? HireDateDisplay : null,
                DateOfBirth = HasDateOfBirth ? DateOfBirthDisplay : null,
                EiExempt = EiExempt
            };

            var id = await _employeeUpdateService.UpdateEmployeeAsync(dto);

            IsSuccess = true;
            Message = $"Employee updated successfully (ID: {id})";
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            Message = ex.Message;
        }
    }

    private void LoadEmployee(EmployeeLookupDto? employee)
    {
        if (employee == null)
            return;

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
        HasHireDate = employee.HireDate.HasValue;
        HireDateDisplay = employee.HireDate?.Date ?? DateTime.Today;
        HasDateOfBirth = employee.DateOfBirth.HasValue;
        DateOfBirthDisplay = employee.DateOfBirth?.Date ?? DateTime.Today.AddYears(-18);
        EiExempt = employee.EiExempt;
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
