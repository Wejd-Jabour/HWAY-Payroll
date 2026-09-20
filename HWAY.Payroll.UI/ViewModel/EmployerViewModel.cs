using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployerInsert;
using HWAY.Payroll.Services.EmployerUpdate;
using HWAY.Payroll.Services.EmployerRemove;
using HWAY.Payroll.Services.PayPeriods;
using HWAY.Payroll.UI.ViewModels;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModel;

public class EmployerViewModel : INotifyPropertyChanged
{
    private readonly IEmployerUpdateService _employerUpdateService;
    private readonly IEmployerInsertService _employerInsertService;
    private readonly IEmployerRemoveService _employerRemoveService;
    private bool _suppressSearchTextChanged;

    public EmployerViewModel(
        IEmployerUpdateService employerUpdateService,
        IEmployerInsertService employerInsertService,
        IEmployerRemoveService employerRemoveService)
    {
        _employerUpdateService = employerUpdateService;
        _employerInsertService = employerInsertService;
        _employerRemoveService = employerRemoveService;

        Employers = new ObservableCollection<EmployerLookupDto>();

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(SelectEmployer);
        SaveCommand = new Command(async () => await SaveEmployerAsync());
        DeleteCommand = new Command(async () => await DeleteEmployerAsync());
        ClearFormCommand = new Command(CancelForm);
        NewEmployerCommand = new Command(() => NewEmployer());
        InitializeCommand = new Command(async () => await InitializeAsync());
    }

    public ICommand InitializeCommand { get; }

    // ===================== Search & Display =====================
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value) && !_suppressSearchTextChanged)
            {
                _ = SearchEmployersAsync();
            }
        }
    }

    public ObservableCollection<EmployerLookupDto> Employers { get; }

    private EmployerLookupDto? _selectedEmployer;
    public EmployerLookupDto? SelectedEmployer
    {
        get => _selectedEmployer;
        set => SetProperty(ref _selectedEmployer, value);
    }

    private string _previousSearchText = "";
    private bool _previousSearchResultsFound;
    private List<EmployerLookupDto> _previousEmployers = new();

    // ===================== Form Fields =====================
    private string _rpNumber = "";
    public string RpNumber
    {
        get => _rpNumber;
        set => SetProperty(ref _rpNumber, value);
    }

    private string _employerName = "";
    public string EmployerName
    {
        get => _employerName;
        set => SetProperty(ref _employerName, value);
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

    private string? _postalCode;
    public string? PostalCode
    {
        get => _postalCode;
        set => SetProperty(ref _postalCode, value);
    }

    private string? _employerGroup;
    public string? EmployerGroup
    {
        get => _employerGroup;
        set
        {
            if (SetProperty(ref _employerGroup, EmployerGroupOption.NormalizeCode(value)))
            {
                OnPropertyChanged(nameof(SelectedEmployerGroupOption));
            }
        }
    }

    public IReadOnlyList<EmployerGroupOption> EmployerGroupOptions => EmployerGroupOption.All;

    public EmployerGroupOption SelectedEmployerGroupOption
    {
        get => EmployerGroupOption.FromCode(EmployerGroup);
        set => EmployerGroup = value?.Code;
    }

    private decimal _fee;
    public decimal Fee
    {
        get => _fee;
        set => SetProperty(ref _fee, value);
    }

    private int _paystubPeriod = 1;
    public int PaystubPeriod
    {
        get => _paystubPeriod;
        set => SetProperty(ref _paystubPeriod, value);
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
                OnPropertyChanged(nameof(CanEditPaystubPeriod));
            }
        }
    }

    public bool CanEditPaystubPeriod => EmployerSelected;

    private bool _searchResultsFound = false;
    public bool SearchResultsFound
    {
        get => _searchResultsFound;
        set => SetProperty(ref _searchResultsFound, value);
    }

    private bool _employerSelected = false;
    public bool EmployerSelected
    {
        get => _employerSelected;
        set
        {
            if (SetProperty(ref _employerSelected, value))
            {
                OnPropertyChanged(nameof(CanEditPaystubPeriod));
            }
        }
    }

    // ===================== Commands =====================
    public ICommand SearchCommand { get; }
    public ICommand SelectEmployerCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearFormCommand { get; }
    public ICommand NewEmployerCommand { get; }

    // ===================== Methods =====================
    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = "";

            _suppressSearchTextChanged = true;
            SearchText = "";
            ClearForm();
            IsEditMode = false;
            EmployerSelected = false;
            _suppressSearchTextChanged = false;

            await LoadEmployersAsync();
            Message = "";
        }
        catch (Exception ex)
        {
            Message = $"Error loading employers: {ex.Message}";
        }
        finally
        {
            _suppressSearchTextChanged = false;
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

            await LoadEmployersAsync();
            Message = Employers.Count == 0 ? "No employers found" : "";
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

    private async Task LoadEmployersAsync()
    {
        var results = await _employerUpdateService.SearchEmployersAsync(SearchText);

        Employers.Clear();
        foreach (var employer in results)
            Employers.Add(employer);

        SearchResultsFound = results.Count > 0;
    }

    private void SelectEmployer(EmployerLookupDto? employer)
    {
        if (employer == null)
            return;

        try
        {
            IsBusy = true;
            Message = "";

            CaptureSearchState();

            SelectedEmployer = employer;

            // Populate form
            RpNumber = employer.RpNumber;
            EmployerName = employer.EmployerName;
            Address = employer.Address ?? "";
            City = employer.City ?? "";
            PostalCode = employer.PostalCode ?? "";
            EmployerGroup = employer.EmployerGroup ?? "";
            PaystubPeriod = employer.PaystubPeriod;
            Fee = employer.Fee;

            IsEditMode = true;
            EmployerSelected = true;
            _suppressSearchTextChanged = true;
            SearchText = ""; // Clear search to show form
            _suppressSearchTextChanged = false;
            Employers.Clear();
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

    private void NewEmployer()
    {
        CaptureSearchState();
        ClearForm();
        IsEditMode = false;
        EmployerSelected = true;
        _suppressSearchTextChanged = true;
        SearchText = "";
        _suppressSearchTextChanged = false;
        Employers.Clear();
        SearchResultsFound = false;
    }

    private void CancelForm()
    {
        ClearForm();
        IsEditMode = false;
        EmployerSelected = false;
        RestoreSearchState();
        Message = "";
    }

    private async Task SaveEmployerAsync()
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

            if (string.IsNullOrWhiteSpace(EmployerName))
            {
                Message = "Employer Name is required";
                return;
            }

            if (!ValidatePaystubPeriod())
            {
                return;
            }

            var savedRpNumber = RpNumber.Trim();

            if (IsEditMode)
            {
                // Update existing employer
                var dto = new EmployerUpdateDto
                {
                    RpNumber = RpNumber,
                    EmployerName = EmployerName,
                    Address = Address,
                    City = City,
                    PostalCode = PostalCode,
                    EmployerGroup = EmployerGroup,
                    PaystubPeriod = PaystubPeriod,
                    Fee = Fee
                };

                await _employerUpdateService.UpdateEmployerAsync(dto);
                Message = "Employer updated successfully";
            }
            else
            {
                // Create new employer
                var dto = new EmployerInsertDto
                {
                    RpNumber = RpNumber,
                    EmployerName = EmployerName,
                    Address = Address,
                    City = City,
                    PostalCode = PostalCode,
                    EmployerGroup = EmployerGroup,
                    PaystubPeriod = PaystubPeriod,
                    Fee = Fee
                };

                await _employerInsertService.CreateEmployerAsync(dto);
                Message = "Employer created successfully";
            }

            var successMessage = Message;

            ClearForm();
            EmployerSelected = false;
            IsEditMode = false;
            _suppressSearchTextChanged = true;
            SearchText = savedRpNumber;
            _suppressSearchTextChanged = false;
            await LoadEmployersAsync();
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

    private async Task DeleteEmployerAsync()
    {
        if (IsBusy || SelectedEmployer == null) return;

        // Show confirmation dialog
        bool confirmed = await Application.Current!.MainPage!.DisplayAlert(
            "Confirm Delete",
            $"Are you sure you want to delete {EmployerName}? This cannot be undone.",
            "Delete",
            "Cancel");

        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            Message = "";

            var dto = new EmployerRemoveDto
            {
                RpNumber = RpNumber,
                EmployerName = EmployerName
            };

            await _employerRemoveService.RemoveEmployerAsync(dto);
            Message = "Employer deleted successfully";

            var successMessage = Message;

            ClearForm();
            EmployerSelected = false;
            IsEditMode = false;
            _suppressSearchTextChanged = true;
            SearchText = "";
            _suppressSearchTextChanged = false;
            await LoadEmployersAsync();
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
        EmployerName = "";
        Address = "";
        City = "";
        PostalCode = "";
        EmployerGroup = EmployerGroupOption.Default.Code;
        PaystubPeriod = 1;
        Fee = 0;
        SelectedEmployer = null;
    }

    private bool ValidatePaystubPeriod()
    {
        var maxPeriod = PayPeriodRules.GetMaxPeriod(EmployerGroup);
        if (PaystubPeriod >= 1 && PaystubPeriod <= maxPeriod)
        {
            return true;
        }

        Message = $"Period # must be between 1 and {maxPeriod} for {SelectedEmployerGroupOption.DisplayName}.";
        return false;
    }

    private void CaptureSearchState()
    {
        _previousSearchText = SearchText;
        _previousSearchResultsFound = SearchResultsFound;
        _previousEmployers = Employers.ToList();
    }

    private void RestoreSearchState()
    {
        _suppressSearchTextChanged = true;
        SearchText = _previousSearchText;
        _suppressSearchTextChanged = false;

        Employers.Clear();
        foreach (var employer in _previousEmployers)
        {
            Employers.Add(employer);
        }

        SearchResultsFound = _previousSearchResultsFound && Employers.Count > 0;
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

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
