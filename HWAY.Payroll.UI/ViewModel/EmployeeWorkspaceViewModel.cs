using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployeeInsert;
using HWAY.Payroll.Services.EmployeeUpdate;
using HWAY.Payroll.Services.EmployeeRemove;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModel;

public class EmployeeWorkspaceViewModel : INotifyPropertyChanged
{
    private readonly IEmployeeUpdateService _employeeUpdateService;
    private readonly IEmployeeInsertService _employeeInsertService;
    private readonly IEmployeeRemoveService _employeeRemoveService;

    public EmployeeWorkspaceViewModel(
        IEmployeeUpdateService employeeUpdateService,
        IEmployeeInsertService employeeInsertService,
        IEmployeeRemoveService employeeRemoveService)
    {
        _employeeUpdateService = employeeUpdateService;
        _employeeInsertService = employeeInsertService;
        _employeeRemoveService = employeeRemoveService;

        Employees = new ObservableCollection<EmployeeLookupDto>();

        SearchCommand = new Command(async () => await SearchEmployeesAsync());
        SelectEmployeeCommand = new Command<EmployeeLookupDto>(async (emp) => await SelectEmployeeAsync(emp));
        SaveCommand = new Command(async () => await SaveEmployeeAsync());
        DeleteCommand = new Command(async () => await DeleteEmployeeAsync());
        ClearFormCommand = new Command(() => ClearForm());
        NewEmployeeCommand = new Command(() => NewEmployee());
        InitializeCommand = new Command(async () => await InitializeAsync());
    }

    public ICommand InitializeCommand { get; }

    // ===================== Search & Display =====================
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public ObservableCollection<EmployeeLookupDto> Employees { get; }

    private EmployeeLookupDto? _selectedEmployee;
    public EmployeeLookupDto? SelectedEmployee
    {
        get => _selectedEmployee;
        set => SetProperty(ref _selectedEmployee, value);
    }

    // ===================== Form Fields =====================
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

    private string _sin = "";
    public string Sin
    {
        get => _sin;
        set => SetProperty(ref _sin, value);
    }

    private string _address = "";
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    private string _city = "";
    public string City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }

    private string _province = "";
    public string Province
    {
        get => _province;
        set => SetProperty(ref _province, value);
    }

    private string _postalCode = "";
    public string PostalCode
    {
        get => _postalCode;
        set => SetProperty(ref _postalCode, value);
    }

    private string _emailAddress = "";
    public string EmailAddress
    {
        get => _emailAddress;
        set => SetProperty(ref _emailAddress, value);
    }

    // ===================== State Management =====================
    private bool _isBusy = false;
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

    private bool _isEditMode = false;
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    private bool _searchResultsFound = false;
    public bool SearchResultsFound
    {
        get => _searchResultsFound;
        set => SetProperty(ref _searchResultsFound, value);
    }

    private bool _employeeSelected = false;
    public bool EmployeeSelected
    {
        get => _employeeSelected;
        set => SetProperty(ref _employeeSelected, value);
    }

    // ===================== Commands =====================
    public ICommand SearchCommand { get; }
    public ICommand SelectEmployeeCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearFormCommand { get; }
    public ICommand NewEmployeeCommand { get; }

    // ===================== Methods =====================
    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = "";

            // Load all employees with empty search
            var results = await _employeeUpdateService.SearchEmployeesAsync("");

            Employees.Clear();
            foreach (var emp in results)
                Employees.Add(emp);

            SearchResultsFound = results.Count > 0;
            Message = "";
        }
        catch (Exception ex)
        {
            Message = $"Error loading employees: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchEmployeesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = "";

            var results = await _employeeUpdateService.SearchEmployeesAsync(SearchText);

            Employees.Clear();
            foreach (var emp in results)
                Employees.Add(emp);

            SearchResultsFound = results.Count > 0;
            Message = results.Count == 0 ? "No employees found" : "";
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

    private async Task SelectEmployeeAsync(EmployeeLookupDto? employee)
    {
        if (employee == null)
            return;

        try
        {
            IsBusy = true;
            Message = "";

            // Load full employee details
            SelectedEmployee = await _employeeUpdateService.GetEmployeeByRpNumberAsync(employee.RpNumber);

            if (SelectedEmployee == null)
            {
                Message = "Failed to load employee details";
                return;
            }

            // Populate form
            RpNumber = SelectedEmployee.RpNumber;
            EmployeeNumber = SelectedEmployee.EmployeeNumber;
            FirstName = SelectedEmployee.FirstName;
            LastName = SelectedEmployee.LastName;
            Sin = SelectedEmployee.Sin ?? "";
            Address = SelectedEmployee.Address ?? "";
            City = SelectedEmployee.City ?? "";
            Province = SelectedEmployee.Province ?? "";
            PostalCode = SelectedEmployee.PostalCode ?? "";
            EmailAddress = SelectedEmployee.EmailAddress ?? "";

            IsEditMode = true;
            EmployeeSelected = true;
            SearchText = ""; // Clear search to show form
            Employees.Clear();
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

    private void NewEmployee()
    {
        ClearForm();
        IsEditMode = false;
        EmployeeSelected = true;
        SearchText = "";
        Employees.Clear();
    }

    private async Task SaveEmployeeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = "";

            // Validation
            if (string.IsNullOrWhiteSpace(RpNumber))
            {
                Message = "RP Number is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(EmployeeNumber))
            {
                Message = "Employee Number is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                Message = "First Name is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                Message = "Last Name is required";
                return;
            }

            if (IsEditMode)
            {
                // Update existing employee
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

                await _employeeUpdateService.UpdateEmployeeAsync(dto);
                Message = "Employee updated successfully";
            }
            else
            {
                // Create new employee
                var dto = new EmployeeInsertDto
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

                await _employeeInsertService.CreateEmployeeAsync(dto);
                Message = "Employee created successfully";
            }

            // Refresh search if there's search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                await SearchEmployeesAsync();
            }

            ClearForm();
            EmployeeSelected = false;
            IsEditMode = false;
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

    private async Task DeleteEmployeeAsync()
    {
        if (IsBusy || SelectedEmployee == null) return;

        // Show confirmation dialog
        bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete {FirstName} {LastName}? This cannot be undone.",
            "Delete",
            "Cancel");

        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            Message = "";

            var dto = new EmployeeRemoveDto
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

            await _employeeRemoveService.RemoveEmployeeAsync(dto);
            Message = "Employee deleted successfully";

            // Refresh search if there's search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                await SearchEmployeesAsync();
            }
            else
            {
                // Load all employees again
                await InitializeAsync();
            }

            ClearForm();
            EmployeeSelected = false;
            IsEditMode = false;
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

    private void ClearForm()
    {
        RpNumber = "";
        EmployeeNumber = "";
        FirstName = "";
        LastName = "";
        Sin = "";
        Address = "";
        City = "";
        Province = "";
        PostalCode = "";
        EmailAddress = "";
        SelectedEmployee = null;
    }

    // ===================== Property Changed =====================
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (Equals(field, value)) return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}
