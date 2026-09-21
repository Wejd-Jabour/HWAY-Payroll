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
    private bool _suppressEmployerSearchTextChanged;

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
        SearchEmployersCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async (employer) => await SelectEmployerAsync(employer));
        SelectEmployeeCommand = new Command<EmployeeLookupDto>(SelectEmployee);
        SaveCommand = new Command(async () => await SaveEmployeeAsync());
        DeleteCommand = new Command(async () => await DeleteEmployeeAsync());
        ClearFormCommand = new Command(CancelForm);
        NewEmployeeCommand = new Command(() => NewEmployee());
        DeselectEmployerCommand = new Command(async () => await InitializeAsync());
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

    private string _employerSearchText = "";
    public string EmployerSearchText
    {
        get => _employerSearchText;
        set
        {
            if (SetProperty(ref _employerSearchText, value) && !_suppressEmployerSearchTextChanged)
            {
                _ = SearchEmployersAsync();
            }
        }
    }

    public ObservableCollection<EmployerLookupDto> Employers { get; }
    public ObservableCollection<EmployeeLookupDto> Employees { get; }

    private EmployerLookupDto? _selectedEmployer;
    public EmployerLookupDto? SelectedEmployer
    {
        get => _selectedEmployer;
        set
        {
            if (SetProperty(ref _selectedEmployer, value))
            {
                OnPropertyChanged(nameof(HasSelectedEmployer));
                OnPropertyChanged(nameof(NeedsEmployerSelection));
                OnPropertyChanged(nameof(SelectedEmployerSummary));
                OnPropertyChanged(nameof(IsRpNumberReadOnly));
            }
        }
    }

    public bool HasSelectedEmployer => SelectedEmployer is not null;
    public bool NeedsEmployerSelection => !HasSelectedEmployer;
    public string SelectedEmployerSummary => SelectedEmployer is null
        ? ""
        : $"{SelectedEmployer.RpNumber} - {SelectedEmployer.EmployerName}";

    private EmployeeLookupDto? _selectedEmployee;
    public EmployeeLookupDto? SelectedEmployee
    {
        get => _selectedEmployee;
        set => SetProperty(ref _selectedEmployee, value);
    }

    private string _previousSearchText = "";
    private bool _previousSearchResultsFound;
    private EmployerLookupDto? _previousSelectedEmployer;
    private List<EmployeeLookupDto> _previousEmployees = new();

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
        set
        {
            if (SetProperty(ref _isEditMode, value))
            {
                OnPropertyChanged(nameof(IsRpNumberReadOnly));
            }
        }
    }

    public bool IsRpNumberReadOnly => IsEditMode || HasSelectedEmployer;

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
    public ICommand SearchEmployersCommand { get; }
    public ICommand SelectEmployerCommand { get; }
    public ICommand SelectEmployeeCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearFormCommand { get; }
    public ICommand NewEmployeeCommand { get; }
    public ICommand DeselectEmployerCommand { get; }

    // ===================== Methods =====================
    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = "";

            _suppressEmployerSearchTextChanged = true;
            EmployerSearchText = "";
            _suppressEmployerSearchTextChanged = false;

            await LoadEmployerOptionsAsync();

            SearchResultsFound = false;
            SelectedEmployer = null;
            EmployeeSelected = false;
            SearchText = "";
            Employees.Clear();
            Message = "";
        }
        catch (Exception ex)
        {
            Message = $"Error loading employers: {ex.Message}";
        }
        finally
        {
            _suppressEmployerSearchTextChanged = false;
            IsBusy = false;
        }
    }

    private async Task SearchEmployersAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = "";

            await LoadEmployerOptionsAsync();
            Message = Employers.Count == 0 ? "No companies found" : "";
        }
        catch (Exception ex)
        {
            Message = $"Error loading companies: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadEmployerOptionsAsync()
    {
        var employers = await _employerLookupService.SearchEmployersAsync(EmployerSearchText);

        Employers.Clear();
        foreach (var employer in employers)
            Employers.Add(employer);
    }

    private async Task SearchEmployeesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = "";

            await LoadEmployeesForSelectedEmployerAsync();
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
        _suppressEmployerSearchTextChanged = true;
        EmployerSearchText = employer.RpNumber;
        _suppressEmployerSearchTextChanged = false;
        SearchText = "";
        ClearForm();
        IsEditMode = false;
        EmployeeSelected = false;
        await SearchEmployeesAsync();
    }

    private void SelectEmployee(EmployeeLookupDto? employee)
    {
        if (employee == null)
            return;

        try
        {
            IsBusy = true;
            Message = "";

            CaptureSearchState();

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
            HasHireDate = employee.HireDate.HasValue;
            HireDateDisplay = employee.HireDate?.Date ?? DateTime.Today;
            HasDateOfBirth = employee.DateOfBirth.HasValue;
            DateOfBirthDisplay = employee.DateOfBirth?.Date ?? DateTime.Today.AddYears(-18);
            EiExempt = employee.EiExempt;

            IsEditMode = true;
            EmployeeSelected = true;
            SearchText = ""; // Clear search to show form
            Employees.Clear();
            SearchResultsFound = false;
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
        if (SelectedEmployer == null)
        {
            Message = "Select a company before adding an employee";
            return;
        }

        CaptureSearchState();
        ClearForm();
        RpNumber = SelectedEmployer.RpNumber;
        IsEditMode = false;
        EmployeeSelected = true;
        SearchText = "";
        Employees.Clear();
        SearchResultsFound = false;
    }

    private void CancelForm()
    {
        ClearForm();
        IsEditMode = false;
        EmployeeSelected = false;
        RestoreSearchState();
        Message = "";
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

            if (!IsEditMode && SelectedEmployer == null)
            {
                Message = "Select a company before adding an employee";
                return;
            }

            if (!IsEditMode && SelectedEmployer != null)
            {
                RpNumber = SelectedEmployer.RpNumber;
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

            if (HasHireDate && HireDateDisplay.Date > DateTime.Today)
            {
                Message = "Hire date cannot be in the future";
                return;
            }

            if (HasDateOfBirth && DateOfBirthDisplay.Date > DateTime.Today)
            {
                Message = "Birth date cannot be in the future";
                return;
            }

            var savedEmployeeNumber = EmployeeNumber.Trim();

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
                    EmailAddress = EmailAddress,
                    HireDate = HasHireDate ? HireDateDisplay : null,
                    DateOfBirth = HasDateOfBirth ? DateOfBirthDisplay : null,
                    EiExempt = EiExempt
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
                    EmailAddress = EmailAddress,
                    HireDate = HasHireDate ? HireDateDisplay : null,
                    DateOfBirth = HasDateOfBirth ? DateOfBirthDisplay : null,
                    EiExempt = EiExempt
                };

                await _employeeInsertService.CreateEmployeeAsync(dto);
                Message = "Employee created successfully";
            }

            var successMessage = Message;

            ClearForm();
            EmployeeSelected = false;
            IsEditMode = false;
            SearchText = savedEmployeeNumber;

            if (SelectedEmployer != null)
            {
                await LoadEmployeesForSelectedEmployerAsync(showNoResultsMessage: false);
            }

            Message = successMessage;
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

            var successMessage = Message;

            ClearForm();
            EmployeeSelected = false;
            IsEditMode = false;
            SearchText = "";

            if (SelectedEmployer != null)
            {
                await LoadEmployeesForSelectedEmployerAsync(showNoResultsMessage: false);
            }

            Message = successMessage;
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
        HasHireDate = false;
        HireDateDisplay = DateTime.Today;
        HasDateOfBirth = false;
        DateOfBirthDisplay = DateTime.Today.AddYears(-18);
        EiExempt = false;
        SelectedEmployee = null;
    }

    private async Task LoadEmployeesForSelectedEmployerAsync(bool showNoResultsMessage = true)
    {
        if (SelectedEmployer == null)
        {
            Employees.Clear();
            SearchResultsFound = false;
            Message = "Select a company before searching employees";
            return;
        }

        var results = await _employeeUpdateService.SearchEmployeesAsync(SearchText, SelectedEmployer.RpNumber);

        Employees.Clear();
        foreach (var emp in results)
            Employees.Add(emp);

        SearchResultsFound = results.Count > 0;
        Message = results.Count == 0 && showNoResultsMessage
            ? "No employees found for this company"
            : "";
    }

    private void CaptureSearchState()
    {
        _previousSearchText = SearchText;
        _previousSearchResultsFound = SearchResultsFound;
        _previousSelectedEmployer = SelectedEmployer;
        _previousEmployees = Employees.ToList();
    }

    private void RestoreSearchState()
    {
        SelectedEmployer = _previousSelectedEmployer;
        SearchText = _previousSearchText;

        Employees.Clear();
        foreach (var employee in _previousEmployees)
        {
            Employees.Add(employee);
        }

        SearchResultsFound = _previousSearchResultsFound && Employees.Count > 0;
    }

    // ===================== Property Changed =====================
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string name = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
