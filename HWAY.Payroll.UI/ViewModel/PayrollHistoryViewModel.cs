using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.Employers;
using HWAY.Payroll.Services.PayPeriods;
using HWAY.Payroll.Services.PayrollHistory;

namespace HWAY.Payroll.UI.ViewModel;

public class PayrollHistoryViewModel : INotifyPropertyChanged
{
    private readonly IEmployerLookupService _employerLookupService;
    private readonly IPayrollHistoryLookupService _payrollHistoryLookupService;
    private bool _suppressSearchTextChanged;
    private bool _suppressFilterRefresh;
    private bool _isLoadingEmployers;
    private bool _isRefreshingHistory;

    public PayrollHistoryViewModel(
        IEmployerLookupService employerLookupService,
        IPayrollHistoryLookupService payrollHistoryLookupService)
    {
        _employerLookupService = employerLookupService;
        _payrollHistoryLookupService = payrollHistoryLookupService;

        Employers = new ObservableCollection<EmployerLookupDto>();
        TermOptions = new ObservableCollection<PayrollHistoryTermOptionDto>();
        YearOptions = new ObservableCollection<int>();
        MonthOptions = new ObservableCollection<PayrollHistoryMonthOptionDto>();
        PeriodOptions = new ObservableCollection<int>();
        Records = new ObservableCollection<PayrollHistoryRecordDto>();

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async employer => await SelectEmployerAsync(employer));
        DeselectEmployerCommand = new Command(ClearForm);
        RefreshHistoryCommand = new Command(async () => await RefreshHistoryAsync());
        ClearCommand = new Command(ClearForm);
    }

    public ObservableCollection<EmployerLookupDto> Employers { get; }
    public ObservableCollection<PayrollHistoryTermOptionDto> TermOptions { get; }
    public ObservableCollection<int> YearOptions { get; }
    public ObservableCollection<PayrollHistoryMonthOptionDto> MonthOptions { get; }
    public ObservableCollection<int> PeriodOptions { get; }
    public ObservableCollection<PayrollHistoryRecordDto> Records { get; }

    public ICommand SearchCommand { get; }
    public ICommand SelectEmployerCommand { get; }
    public ICommand DeselectEmployerCommand { get; }
    public ICommand RefreshHistoryCommand { get; }
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
        set
        {
            if (SetProperty(ref _selectedEmployer, value))
            {
                OnPropertyChanged(nameof(HasSelectedEmployer));
                OnPropertyChanged(nameof(NeedsEmployerSelection));
            }
        }
    }

    public bool HasSelectedEmployer => SelectedEmployer is not null;
    public bool NeedsEmployerSelection => !HasSelectedEmployer;

    private PayrollHistoryTermOptionDto? _selectedTerm;
    public PayrollHistoryTermOptionDto? SelectedTerm
    {
        get => _selectedTerm;
        set
        {
            if (SetProperty(ref _selectedTerm, value) && !_suppressFilterRefresh)
            {
                _ = OnTermChangedAsync();
            }
        }
    }

    private int? _selectedYear;
    public int? SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (SetProperty(ref _selectedYear, value) && !_suppressFilterRefresh)
            {
                _ = OnYearChangedAsync();
            }
        }
    }

    private PayrollHistoryMonthOptionDto? _selectedMonth;
    public PayrollHistoryMonthOptionDto? SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value) && !_suppressFilterRefresh)
            {
                _ = OnMonthChangedAsync();
            }
        }
    }

    private int? _selectedPeriod;
    public int? SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (SetProperty(ref _selectedPeriod, value) && !_suppressFilterRefresh)
            {
                _ = RefreshHistoryAsync();
            }
        }
    }

    private string _employeeSearchText = string.Empty;
    public string EmployeeSearchText
    {
        get => _employeeSearchText;
        set => SetProperty(ref _employeeSearchText, value);
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

    private bool _searchResultsFound;
    public bool SearchResultsFound
    {
        get => _searchResultsFound;
        set => SetProperty(ref _searchResultsFound, value);
    }

    private bool _historyNotFound;
    public bool HistoryNotFound
    {
        get => _historyNotFound;
        set => SetProperty(ref _historyNotFound, value);
    }

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private int _recordCount;
    public int RecordCount
    {
        get => _recordCount;
        set => SetProperty(ref _recordCount, value);
    }

    private int _employeeCount;
    public int EmployeeCount
    {
        get => _employeeCount;
        set => SetProperty(ref _employeeCount, value);
    }

    private decimal _gross;
    public decimal Gross
    {
        get => _gross;
        set => SetProperty(ref _gross, value);
    }

    private decimal _totalPay;
    public decimal TotalPay
    {
        get => _totalPay;
        set => SetProperty(ref _totalPay, value);
    }

    private decimal _totalDeductions;
    public decimal TotalDeductions
    {
        get => _totalDeductions;
        set => SetProperty(ref _totalDeductions, value);
    }

    private decimal _netPay;
    public decimal NetPay
    {
        get => _netPay;
        set => SetProperty(ref _netPay, value);
    }

    public async Task InitializeAsync()
    {
        if (IsBusy || _isLoadingEmployers || _isRefreshingHistory)
            return;

        _suppressSearchTextChanged = true;
        _suppressFilterRefresh = true;

        SearchText = string.Empty;
        EmployeeSearchText = string.Empty;
        SelectedEmployer = null;
        ClearFilterOptions();
        ClearResults();
        IsDropdownVisible = false;
        Message = string.Empty;

        _suppressFilterRefresh = false;
        _suppressSearchTextChanged = false;

        await SearchEmployersAsync();
    }

    private async Task SearchEmployersAsync()
    {
        if (_isLoadingEmployers)
            return;

        try
        {
            _isLoadingEmployers = true;
            IsBusy = true;
            Message = string.Empty;

            var results = await _employerLookupService.SearchEmployersAsync(SearchText);

            Employers.Clear();
            foreach (var employer in results)
            {
                Employers.Add(employer);
            }

            IsDropdownVisible = NeedsEmployerSelection && Employers.Count > 0;
        }
        catch (Exception ex)
        {
            Message = $"Error loading companies: {ex.Message}";
            IsDropdownVisible = false;
        }
        finally
        {
            IsBusy = false;
            _isLoadingEmployers = false;
        }
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

            await LoadFilterOptionsAsync();
        }
        catch (Exception ex)
        {
            Message = $"Error selecting company: {ex.Message}";
        }
        finally
        {
            _suppressSearchTextChanged = false;
            IsBusy = false;
        }

        await RefreshHistoryAsync();
    }

    private async Task LoadFilterOptionsAsync()
    {
        _suppressFilterRefresh = true;

        ClearFilterOptions();
        ClearResults();

        if (SelectedEmployer == null)
        {
            _suppressFilterRefresh = false;
            return;
        }

        try
        {
            var terms = await _payrollHistoryLookupService.GetTermOptionsAsync(SelectedEmployer.RpNumber);

            foreach (var term in terms)
            {
                TermOptions.Add(term);
            }

            var employerGroup = PayPeriodRules.NormalizeEmployerGroup(SelectedEmployer.EmployerGroup);
            SelectedTerm = TermOptions.FirstOrDefault(term => term.Value == employerGroup)
                ?? TermOptions.FirstOrDefault();

            if (SelectedTerm == null)
            {
                HistoryNotFound = true;
                Message = "No payroll history has been saved for this company.";
                return;
            }

            await ReloadYearsAsync();
        }
        finally
        {
            _suppressFilterRefresh = false;
        }
    }

    private async Task OnTermChangedAsync()
    {
        await ReloadFiltersAndRefreshAsync(async () => await ReloadYearsAsync());
    }

    private async Task OnYearChangedAsync()
    {
        await ReloadFiltersAndRefreshAsync(async () =>
        {
            ClearMonthAndPeriodOptions();

            if (SelectedYear.HasValue)
            {
                await ReloadMonthsAsync();
            }
        });
    }

    private async Task OnMonthChangedAsync()
    {
        await ReloadFiltersAndRefreshAsync(async () =>
        {
            ClearPeriodOptions();

            if (SelectedYear.HasValue)
            {
                await ReloadPeriodsAsync();
            }
        });
    }

    private async Task ReloadFiltersAndRefreshAsync(Func<Task> reload)
    {
        if (SelectedEmployer == null)
            return;

        try
        {
            IsBusy = true;
            _suppressFilterRefresh = true;
            Message = string.Empty;

            await reload();
        }
        catch (Exception ex)
        {
            Message = $"Error loading history filters: {ex.Message}";
        }
        finally
        {
            _suppressFilterRefresh = false;
            IsBusy = false;
        }

        await RefreshHistoryAsync();
    }

    private async Task ReloadYearsAsync()
    {
        ClearYearMonthAndPeriodOptions();

        if (SelectedEmployer == null)
            return;

        var years = await _payrollHistoryLookupService.GetYearsAsync(
            SelectedEmployer.RpNumber,
            SelectedTerm?.Value);

        foreach (var year in years)
        {
            YearOptions.Add(year);
        }

        SelectedYear = YearOptions.Count > 0 ? YearOptions[0] : null;

        if (SelectedYear.HasValue)
        {
            await ReloadMonthsAsync();
        }
    }

    private async Task ReloadMonthsAsync()
    {
        ClearMonthAndPeriodOptions();

        if (SelectedEmployer == null || !SelectedYear.HasValue)
            return;

        var months = await _payrollHistoryLookupService.GetMonthsAsync(
            SelectedEmployer.RpNumber,
            SelectedTerm?.Value,
            SelectedYear.Value);

        foreach (var month in months)
        {
            MonthOptions.Add(month);
        }

        SelectedMonth = MonthOptions.Count > 0
            ? MonthOptions[MonthOptions.Count - 1]
            : null;

        if (SelectedMonth != null)
        {
            await ReloadPeriodsAsync();
        }
    }

    private async Task ReloadPeriodsAsync()
    {
        ClearPeriodOptions();

        if (SelectedEmployer == null || !SelectedYear.HasValue)
            return;

        var periods = await _payrollHistoryLookupService.GetPeriodsAsync(
            SelectedEmployer.RpNumber,
            SelectedTerm?.Value,
            SelectedYear.Value,
            SelectedMonth?.Number);

        foreach (var period in periods)
        {
            PeriodOptions.Add(period);
        }

        SelectedPeriod = PeriodOptions.Count > 0
            ? PeriodOptions[PeriodOptions.Count - 1]
            : null;
    }

    private async Task RefreshHistoryAsync()
    {
        if (_isRefreshingHistory)
            return;

        if (SelectedEmployer == null)
        {
            ClearResults();
            Message = "Select a company first.";
            return;
        }

        if (SelectedTerm == null || !SelectedYear.HasValue)
        {
            ClearResults();
            HistoryNotFound = true;
            Message = "No payroll history has been saved for this company.";
            return;
        }

        try
        {
            _isRefreshingHistory = true;
            IsBusy = true;
            Message = string.Empty;

            var result = await _payrollHistoryLookupService.SearchAsync(new PayrollHistorySearchRequest
            {
                RpNumber = SelectedEmployer.RpNumber,
                EmployerGroup = SelectedTerm.Value,
                Year = SelectedYear,
                Month = SelectedMonth?.Number,
                Period = SelectedPeriod,
                EmployeeSearchText = EmployeeSearchText
            });

            Records.Clear();
            foreach (var record in result.Records)
            {
                Records.Add(record);
            }

            RecordCount = result.RecordCount;
            EmployeeCount = result.EmployeeCount;
            Gross = result.Gross;
            TotalPay = result.TotalPay;
            TotalDeductions = result.TotalDeductions;
            NetPay = result.NetPay;
            SearchResultsFound = Records.Count > 0;
            HistoryNotFound = !SearchResultsFound;

            if (!SearchResultsFound)
            {
                Message = "No payroll history found for the selected filters.";
            }
        }
        catch (Exception ex)
        {
            ClearResults();
            Message = $"Error loading payroll history: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            _isRefreshingHistory = false;
        }
    }

    private void ClearForm()
    {
        _suppressSearchTextChanged = true;
        _suppressFilterRefresh = true;

        SearchText = string.Empty;
        EmployeeSearchText = string.Empty;
        SelectedEmployer = null;
        ClearFilterOptions();
        ClearResults();
        IsDropdownVisible = NeedsEmployerSelection && Employers.Count > 0;
        Message = string.Empty;

        _suppressFilterRefresh = false;
        _suppressSearchTextChanged = false;

        _ = SearchEmployersAsync();
    }

    private void ClearFilterOptions()
    {
        TermOptions.Clear();
        SelectedTerm = null;
        ClearYearMonthAndPeriodOptions();
    }

    private void ClearYearMonthAndPeriodOptions()
    {
        YearOptions.Clear();
        SelectedYear = null;
        ClearMonthAndPeriodOptions();
    }

    private void ClearMonthAndPeriodOptions()
    {
        MonthOptions.Clear();
        SelectedMonth = null;
        ClearPeriodOptions();
    }

    private void ClearPeriodOptions()
    {
        PeriodOptions.Clear();
        SelectedPeriod = null;
    }

    private void ClearResults()
    {
        Records.Clear();
        RecordCount = 0;
        EmployeeCount = 0;
        Gross = 0;
        TotalPay = 0;
        TotalDeductions = 0;
        NetPay = 0;
        SearchResultsFound = false;
        HistoryNotFound = false;
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
