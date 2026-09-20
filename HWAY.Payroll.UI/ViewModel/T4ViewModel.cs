using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.Employers;
using HWAY.Payroll.Services.T4;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModel;

public class T4ViewModel : INotifyPropertyChanged
{
    private readonly IT4LookupService _t4LookupService;
    private readonly IEmployerLookupService _employerLookupService;
    private bool _suppressSearchTextChanged;
    private bool _isCalculating;
    private bool _isLoadingEmployers;

    public T4ViewModel(
        IT4LookupService t4LookupService,
        IEmployerLookupService employerLookupService)
    {
        _t4LookupService = t4LookupService;
        _employerLookupService = employerLookupService;

        Employers = new ObservableCollection<EmployerLookupDto>();
        Slips = new ObservableCollection<T4EmployeeSlipDto>();

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async employer => await SelectEmployerAsync(employer));
        ClearCommand = new Command(async () => await InitializeAsync());

        Year = DateTime.Today.Year;
    }

    public ObservableCollection<EmployerLookupDto> Employers { get; }
    public ObservableCollection<T4EmployeeSlipDto> Slips { get; }

    public ICommand SearchCommand { get; }
    public ICommand SelectEmployerCommand { get; }
    public ICommand ClearCommand { get; }

    private string _searchText = string.Empty;
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

    private EmployerLookupDto? _selectedEmployer;
    public EmployerLookupDto? SelectedEmployer
    {
        get => _selectedEmployer;
        set => SetProperty(ref _selectedEmployer, value);
    }

    private bool _isDropdownVisible;
    public bool IsDropdownVisible
    {
        get => _isDropdownVisible;
        set => SetProperty(ref _isDropdownVisible, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private int _year;
    public int Year
    {
        get => _year;
        set
        {
            if (SetProperty(ref _year, value))
            {
                _ = RefreshT4Async();
            }
        }
    }

    private string _calculationSource = string.Empty;
    public string CalculationSource
    {
        get => _calculationSource;
        set => SetProperty(ref _calculationSource, value);
    }

    private decimal _box14Total;
    public decimal Box14Total
    {
        get => _box14Total;
        set => SetProperty(ref _box14Total, value);
    }

    private decimal _box24Total;
    public decimal Box24Total
    {
        get => _box24Total;
        set => SetProperty(ref _box24Total, value);
    }

    private decimal _box26Total;
    public decimal Box26Total
    {
        get => _box26Total;
        set => SetProperty(ref _box26Total, value);
    }

    private decimal _box22Total;
    public decimal Box22Total
    {
        get => _box22Total;
        set => SetProperty(ref _box22Total, value);
    }

    private decimal _box16Total;
    public decimal Box16Total
    {
        get => _box16Total;
        set => SetProperty(ref _box16Total, value);
    }

    private decimal _box16ATotal;
    public decimal Box16ATotal
    {
        get => _box16ATotal;
        set => SetProperty(ref _box16ATotal, value);
    }

    private decimal _box18Total;
    public decimal Box18Total
    {
        get => _box18Total;
        set => SetProperty(ref _box18Total, value);
    }

    private T4TaxYearRules? _taxYearRules;
    public T4TaxYearRules? TaxYearRules
    {
        get => _taxYearRules;
        set
        {
            if (SetProperty(ref _taxYearRules, value))
            {
                OnPropertyChanged(nameof(TaxYearMaximums));
            }
        }
    }

    public bool HasT4Results => Slips.Count > 0;

    public string TaxYearMaximums
    {
        get
        {
            if (TaxYearRules == null)
                return string.Empty;

            return $"EI max earnings {TaxYearRules.EiMaximumInsurableEarnings:0.00} | CPP YMPE {TaxYearRules.CppMaximumPensionableEarnings:0.00} | CPP2/YAMPE {TaxYearRules.CppAdditionalMaximumPensionableEarnings:0.00}";
        }
    }

    public async Task InitializeAsync()
    {
        if (_isLoadingEmployers || _isCalculating)
            return;

        ResetFormState();

        try
        {
            IsBusy = true;
            await LoadEmployerOptionsAsync();
            Message = string.Empty;
        }
        catch (Exception ex)
        {
            Message = $"Error loading employers: {ex.Message}";
            IsDropdownVisible = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchEmployersAsync()
    {
        if (_isLoadingEmployers)
            return;

        var shouldSetBusy = !IsBusy;

        try
        {
            _isLoadingEmployers = true;
            if (shouldSetBusy)
            {
                IsBusy = true;
            }

            Message = string.Empty;
            await LoadEmployerOptionsAsync();
        }
        catch (Exception ex)
        {
            Message = $"Error loading employers: {ex.Message}";
            IsDropdownVisible = false;
        }
        finally
        {
            if (shouldSetBusy)
            {
                IsBusy = false;
            }

            _isLoadingEmployers = false;
        }
    }

    private async Task LoadEmployerOptionsAsync()
    {
        var results = await _employerLookupService.SearchEmployersAsync(SearchText);

        Employers.Clear();
        foreach (var employer in results)
        {
            Employers.Add(employer);
        }

        IsDropdownVisible = Employers.Count > 0;
    }

    private async Task SelectEmployerAsync(EmployerLookupDto? employer)
    {
        if (employer == null)
            return;

        try
        {
            IsBusy = true;
            Message = string.Empty;
            IsDropdownVisible = false;

            SelectedEmployer = await _employerLookupService.GetEmployerByRpNumberAsync(employer.RpNumber) ?? employer;

            _suppressSearchTextChanged = true;
            SearchText = SelectedEmployer.RpNumber;
            _suppressSearchTextChanged = false;
        }
        catch (Exception ex)
        {
            Message = $"Error selecting employer: {ex.Message}";
        }
        finally
        {
            _suppressSearchTextChanged = false;
            IsBusy = false;
        }

        await RefreshT4Async();
    }

    private async Task RefreshT4Async()
    {
        if (_isCalculating || SelectedEmployer == null || Year < 1)
        {
            if (SelectedEmployer == null)
                ClearCalculatedFields();

            return;
        }

        try
        {
            _isCalculating = true;
            IsBusy = true;
            Message = string.Empty;

            var calculated = await _t4LookupService.CalculateCompanyT4Async(SelectedEmployer.RpNumber, Year);
            ApplyCalculatedFields(calculated);
        }
        catch (Exception ex)
        {
            ClearCalculatedFields();
            Message = $"Error calculating T4: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            _isCalculating = false;
        }
    }

    private void ApplyCalculatedFields(T4CompanyDto calculated)
    {
        CalculationSource = calculated.CalculationSource;
        TaxYearRules = calculated.TaxYearRules;
        Box14Total = calculated.Box14Total;
        Box24Total = calculated.Box24Total;
        Box26Total = calculated.Box26Total;
        Box22Total = calculated.Box22Total;
        Box16Total = calculated.Box16Total;
        Box16ATotal = calculated.Box16ATotal;
        Box18Total = calculated.Box18Total;

        Slips.Clear();
        foreach (var slip in calculated.Slips)
        {
            Slips.Add(slip);
        }

        OnPropertyChanged(nameof(HasT4Results));

        if (Slips.Count == 0)
        {
            Message = "No payroll information found for this company.";
        }
    }

    private void ClearCalculatedFields()
    {
        CalculationSource = string.Empty;
        TaxYearRules = null;
        Box14Total = 0;
        Box24Total = 0;
        Box26Total = 0;
        Box22Total = 0;
        Box16Total = 0;
        Box16ATotal = 0;
        Box18Total = 0;
        Slips.Clear();
        OnPropertyChanged(nameof(HasT4Results));
    }

    private void ResetFormState()
    {
        _suppressSearchTextChanged = true;
        SearchText = string.Empty;
        SelectedEmployer = null;
        IsDropdownVisible = false;
        Year = DateTime.Today.Year;
        ClearCalculatedFields();
        Message = string.Empty;
        _suppressSearchTextChanged = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
