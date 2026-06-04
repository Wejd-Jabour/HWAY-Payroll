using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployerInsert;
using HWAY.Payroll.Services.EmployerUpdate;
using HWAY.Payroll.Services.EmployerRemove;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModel;

public class EmployerViewModel : INotifyPropertyChanged
{
    private readonly IEmployerUpdateService _employerUpdateService;
    private readonly IEmployerInsertService _employerInsertService;
    private readonly IEmployerRemoveService _employerRemoveService;

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
        SelectEmployerCommand = new Command<EmployerLookupDto>(async (employer) => await SelectEmployerAsync(employer));
        SaveCommand = new Command(async () => await SaveEmployerAsync());
        DeleteCommand = new Command(async () => await DeleteEmployerAsync());
        ClearFormCommand = new Command(() => ClearForm());
        NewEmployerCommand = new Command(() => NewEmployer());
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

    private EmployerLookupDto? _selectedEmployer;
    public EmployerLookupDto? SelectedEmployer
    {
        get => _selectedEmployer;
        set => SetProperty(ref _selectedEmployer, value);
    }

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
        set => SetProperty(ref _employerGroup, value);
    }

    private bool? _isActive = true;
    public bool? IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
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

    private bool _employerSelected = false;
    public bool EmployerSelected
    {
        get => _employerSelected;
        set => SetProperty(ref _employerSelected, value);
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

            // Load all employers on initialization
            var employers = await _employerUpdateService.SearchEmployersAsync("");

            Employers.Clear();
            foreach (var employer in employers)
                Employers.Add(employer);

            SearchResultsFound = employers.Count > 0;
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

    private async Task SearchEmployersAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = "";

            var results = await _employerUpdateService.SearchEmployersAsync(SearchText);

            Employers.Clear();
            foreach (var employer in results)
                Employers.Add(employer);

            SearchResultsFound = results.Count > 0;
            Message = results.Count == 0 ? "No employers found" : "";
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

        try
        {
            IsBusy = true;
            Message = "";

            SelectedEmployer = employer;

            // Populate form
            RpNumber = employer.RpNumber;
            EmployerName = employer.EmployerName;
            IsActive = employer.IsActive ?? true;
            Address = employer.Address ?? "";
            City = employer.City ?? "";
            PostalCode = employer.PostalCode ?? "";
            EmployerGroup = employer.EmployerGroup ?? "";

            IsEditMode = true;
            EmployerSelected = true;
            SearchText = ""; // Clear search to show form
            Employers.Clear();
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
        ClearForm();
        IsEditMode = false;
        EmployerSelected = true;
        SearchText = "";
        Employers.Clear();
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

            if (IsEditMode)
            {
                // Update existing employer
                var dto = new EmployerUpdateDto
                {
                    RpNumber = RpNumber,
                    EmployerName = EmployerName,
                    IsActive = IsActive,
                    Address = Address,
                    City = City,
                    PostalCode = PostalCode,
                    EmployerGroup = EmployerGroup
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
                    IsActive = IsActive,
                    Address = Address,
                    City = City,
                    PostalCode = PostalCode,
                    EmployerGroup = EmployerGroup
                };

                await _employerInsertService.CreateEmployerAsync(dto);
                Message = "Employer created successfully";
            }

            // Refresh search if there's search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                await SearchEmployersAsync();
            }

            ClearForm();
            EmployerSelected = false;
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

            // Refresh search if there's search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                await SearchEmployersAsync();
            }
            else
            {
                // Load all employers again
                await InitializeAsync();
            }

            ClearForm();
            EmployerSelected = false;
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
        EmployerName = "";
        IsActive = true;
        Address = "";
        City = "";
        PostalCode = "";
        EmployerGroup = "";
        SelectedEmployer = null;
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
