using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployeeInsert;
using HWAY.Payroll.Services.EmployeeUpdate;
using HWAY.Payroll.Services.EmployeeRemove;
using HWAY.Payroll.Services.Employers;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModel;

public class EmployeeWorkspaceViewModel : INotifyPropertyChanged
{
    private readonly IEmployeeUpdateService _employeeUpdateService;
    private readonly IEmployeeInsertService _employeeInsertService;
    private readonly IEmployeeRemoveService _employeeRemoveService;
    private readonly IEmployerLookupService _employerLookupService;

    public EmployeeWorkspaceViewModel(
        IEmployeeUpdateService employeeUpdateService,
        IEmployeeInsertService employeeInsertService,
        IEmployeeRemoveService employeeRemoveService,
        IEmployerLookupService employerLookupService)
    {
        _employeeUpdateService = employeeUpdateService;
        _employeeInsertService = employeeInsertService;
        _employeeRemoveService = employeeRemoveService;
        _employerLookupService = employerLookupService;

        Employers = new ObservableCollection<EmployerLookupDto>();
        Employees = new ObservableCollection<EmployeeLookupDto>();

        SearchCommand = new Command(async () => await SearchEmployeesAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async (employer) => await SelectEmployerAsync(employer));
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

    public ObservableCollection<EmployerLookupDto> Employers { get; }
    public ObservableCollection<EmployeeLookupDto> Employees { get; }

    private EmployerLookupDto? _selectedEmployer;
    public EmployerLookupDto? SelectedEmployer
    {
        get => _selectedEmployer;
        set => SetProperty(ref _selectedEmployer, value);
    }

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
    public ICommand SelectEmployerCommand { get; }
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

            // Load all employers in the top selection box
            var employers = await _employerLookupService.SearchEmployersAsync("");

            Employers.Clear();
            foreach (var employer in employers)
                Employers.Add(employer);

            SearchResultsFound = false;
            SelectedEmployer = null;
            Message = "";
        }
        catch (Exception ex)
        {
            Message = $"Error loading employers: {ex.Message}";
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

    private async Task SelectEmployerAsync(EmployerLookupDto? employer)
    {
        if (employer == null)
            return;

        SelectedEmployer = employer;
        SearchText = employer.RpNumber;
        await SearchEmployeesAsync();
    }

    private async Task SelectEmployeeAsync(EmployeeLookupDto? employee)
    {
        if (employee == null)
            return;

        try
        {
            IsBusy = true;
            Message = "";

            SelectedEmployee = employee;

            // Populate form
            RpNumber = employee.RpNumber;
            EmployeeNumber = employee.EmployeeNumber;
            FirstName = employee.FirstName;
            LastName = employee.LastName;
            Sin = employee.Sin ?? "";
            Address = employee.Address ?? "";
            City = employee.City ?? "";
            Province = employee.Province ?? "";
            PostalCode = employee.PostalCode ?? "";
            EmailAddress = employee.EmailAddress ?? "";

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
