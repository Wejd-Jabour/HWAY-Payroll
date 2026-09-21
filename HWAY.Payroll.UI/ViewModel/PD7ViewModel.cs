using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.Employers;
using HWAY.Payroll.Services.PD7;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace HWAY.Payroll.UI.ViewModel;

public class PD7ViewModel : INotifyPropertyChanged
{
    private readonly IPD7LookupService _pd7LookupService;
    private readonly IEmployerLookupService _employerLookupService;
    private bool _suppressSearchTextChanged;
    private bool _suppressCalculations;
    private bool _isCalculating;
    private bool _isLoadingEmployers;

    public PD7ViewModel(
        IPD7LookupService pd7LookupService,
        IEmployerLookupService employerLookupService)
    {
        _pd7LookupService = pd7LookupService;
        _employerLookupService = employerLookupService;

        Employers = new ObservableCollection<EmployerLookupDto>();
        Months = MonthOption.All;
        YearOptions = BuildYearOptions();

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async employer => await SelectEmployerAsync(employer));
        DeselectEmployerCommand = new Command(async () => await InitializeAsync());
        ExportPd7CsvCommand = new Command(async () => await ExportPd7CsvAsync());
        SaveCommand = new Command(async () => await SaveAsync());
        ClearCommand = new Command(async () => await InitializeAsync());

        _suppressCalculations = true;
        Period = "XX";
        Year = DateTime.Today.Year;
        SelectedMonth = Months.First(month => month.Number == DateTime.Today.Month);
        _suppressCalculations = false;

    }

    public ObservableCollection<EmployerLookupDto> Employers { get; }
    public IReadOnlyList<MonthOption> Months { get; }
    public IReadOnlyList<int> YearOptions { get; }

    public ICommand SearchCommand { get; }
    public ICommand SelectEmployerCommand { get; }
    public ICommand DeselectEmployerCommand { get; }
    public ICommand ExportPd7CsvCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ClearCommand { get; }

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

    private bool _isDropdownVisible;
    public bool IsDropdownVisible
    {
        get => _isDropdownVisible;
        set => SetProperty(ref _isDropdownVisible, value);
    }

    private string _rpNumber = "";
    public string RpNumber
    {
        get => _rpNumber;
        set => SetProperty(ref _rpNumber, value);
    }

    private decimal _fee;
    public decimal Fee
    {
        get => _fee;
        set => SetProperty(ref _fee, value);
    }

    private decimal _gross;
    public decimal Gross
    {
        get => _gross;
        set
        {
            if (SetProperty(ref _gross, value))
            {
                NotifyCalculationCheckChanged();
            }
        }
    }

    private decimal _netTotal;
    public decimal NetTotal
    {
        get => _netTotal;
        set
        {
            if (SetProperty(ref _netTotal, value))
            {
                NotifyCalculationCheckChanged();
            }
        }
    }

    private decimal _employerContribution;
    public decimal EmployerContribution
    {
        get => _employerContribution;
        set
        {
            if (SetProperty(ref _employerContribution, value))
            {
                NotifyCalculationCheckChanged();
            }
        }
    }

    private decimal _pd7Total;
    public decimal PD7Total
    {
        get => _pd7Total;
        set
        {
            if (SetProperty(ref _pd7Total, value))
            {
                NotifyCalculationCheckChanged();
            }
        }
    }

    private decimal _calendarYearPD7Total;
    public decimal CalendarYearPD7Total
    {
        get => _calendarYearPD7Total;
        set => SetProperty(ref _calendarYearPD7Total, value);
    }

    private int _activeEmployees;
    public int ActiveEmployees
    {
        get => _activeEmployees;
        set => SetProperty(ref _activeEmployees, value);
    }

    private string _period = "";
    public string Period
    {
        get => _period;
        set
        {
            if (SetProperty(ref _period, value))
            {
                OnPropertyChanged(nameof(PeriodSummary));
            }
        }
    }

    public string PeriodSummary => string.IsNullOrWhiteSpace(Period) ? "XX" : Period;

    private decimal _transfer;
    public decimal Transfer
    {
        get => _transfer;
        set
        {
            if (SetProperty(ref _transfer, value))
            {
                UpdateFeeCheck();
            }
        }
    }

    private decimal _paidSoFar;
    public decimal PaidSoFar
    {
        get => _paidSoFar;
        set
        {
            if (SetProperty(ref _paidSoFar, value))
            {
                UpdateFeeCheck();
            }
        }
    }

    private decimal _received;
    public decimal Received
    {
        get => _received;
        set
        {
            if (SetProperty(ref _received, value))
            {
                UpdateFeeCheck();
            }
        }
    }

    public decimal TotalReceived => RoundMoney(PaidSoFar + Received);
    public decimal OutstandingBalance => Math.Max(RoundMoney(Transfer - TotalReceived), 0);
    public bool HasPaidSoFar => RoundMoney(PaidSoFar) > 0;

    private bool _feeCheck;
    public bool FeeCheck
    {
        get => _feeCheck;
        set => SetProperty(ref _feeCheck, value);
    }

    public bool CalculationCheck => RoundMoney(NetTotal + PD7Total) == RoundMoney(Gross + EmployerContribution);
    public decimal CalculationCheckDifference => RoundMoney((NetTotal + PD7Total) - (Gross + EmployerContribution));

    public Color ReceivedStatusColor
    {
        get
        {
            if (Transfer == 0 && TotalReceived == 0)
                return Colors.Transparent;

            return FeeCheck
                ? Color.FromArgb("#D8F3DC")
                : Color.FromArgb("#F8D7DA");
        }
    }

    public string FeeCheckStatusText
    {
        get
        {
            if (Transfer == 0 && TotalReceived == 0)
                return "";

            return FeeCheck
                ? "Total received matches transfer"
                : "Total received does not match transfer";
        }
    }

    public Color CalculationCheckStatusColor
    {
        get
        {
            if (!HasCalculationAmounts())
                return Colors.Transparent;

            return CalculationCheck
                ? Color.FromArgb("#D8F3DC")
                : Color.FromArgb("#F8D7DA");
        }
    }

    public string CalculationCheckStatusText
    {
        get
        {
            if (!HasCalculationAmounts())
                return "";

            return CalculationCheck
                ? "Net + PD7 matches gross + employer contribution"
                : $"Calculation check off by {Math.Abs(CalculationCheckDifference):0.00}";
        }
    }

    private MonthOption? _selectedMonth;
    public MonthOption? SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value))
            {
                Month = value?.Number ?? 0;
                Period = "XX";
                Received = 0;
                PaidSoFar = 0;

                if (!_suppressCalculations)
                {
                    _ = RefreshCalculationsAsync();
                }
            }
        }
    }

    private int _month;
    public int Month
    {
        get => _month;
        set => SetProperty(ref _month, value);
    }

    private int _year;
    public int Year
    {
        get => _year;
        set
        {
            if (SetProperty(ref _year, value))
            {
                Received = 0;
                PaidSoFar = 0;

                if (!_suppressCalculations)
                {
                    _ = RefreshCalculationsAsync();
                }
            }
        }
    }

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

    public async Task InitializeAsync()
    {
        if (_isLoadingEmployers || _isCalculating)
            return;

        ResetFormState();

        try
        {
            IsBusy = true;
            await LoadEmployerOptionsAsync();
            Message = "";
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

            Message = "";
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

        IsDropdownVisible = NeedsEmployerSelection && Employers.Count > 0;
    }

    private async Task SelectEmployerAsync(EmployerLookupDto? employer)
    {
        if (employer == null)
            return;

        try
        {
            IsBusy = true;
            Message = "";
            IsDropdownVisible = false;

            SelectedEmployer = await _employerLookupService.GetEmployerByRpNumberAsync(employer.RpNumber) ?? employer;
            RpNumber = SelectedEmployer.RpNumber;
            Fee = SelectedEmployer.Fee;
            Received = 0;
            PaidSoFar = 0;

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

        await RefreshCalculationsAsync();
    }

    private async Task RefreshCalculationsAsync()
    {
        if (_suppressCalculations || _isCalculating)
            return;

        if (string.IsNullOrWhiteSpace(RpNumber) || Month < 1 || Year < 1)
        {
            ClearCalculatedFields();
            return;
        }

        try
        {
            _isCalculating = true;
            IsBusy = true;
            Message = "";

            var calculated = await _pd7LookupService.CalculatePd7Async(RpNumber, Month, Year);
            ApplyCalculatedFields(calculated);

            var saved = await _pd7LookupService.GetByPeriodAsync(RpNumber, Month, Year);
            if (saved != null)
            {
                ApplySavedFields(saved);
            }

            await RefreshCalendarYearPD7TotalAsync();
        }
        catch (Exception ex)
        {
            ClearCalculatedFields();
            Message = $"Error calculating PD7: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            _isCalculating = false;
        }
    }

    private async Task RefreshCalendarYearPD7TotalAsync()
    {
        if (string.IsNullOrWhiteSpace(RpNumber) || Year < 1)
        {
            CalendarYearPD7Total = 0;
            return;
        }

        CalendarYearPD7Total = await _pd7LookupService.GetCalendarYearPd7TotalAsync(RpNumber, Year);
    }

    private async Task ExportPd7CsvAsync()
    {
        if (IsBusy)
            return;

        if (SelectedMonth == null || Month < 1 || Month > 12)
        {
            Message = "Select a month first.";
            return;
        }

        if (Year < 1)
        {
            Message = "Select a year first.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = "Creating PD7 CSV...";

            var filePath = await _pd7LookupService.ExportMonthlyPd7CsvAsync(Month, Year);
            Message = $"PD7 CSV created for {SelectedMonth.Name} {Year}.";

            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
        }
        catch (Exception ex)
        {
            Message = $"Unable to print PD7 CSV: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = "";

            if (SelectedEmployer == null || string.IsNullOrWhiteSpace(RpNumber))
            {
                Message = "Select a company first.";
                return;
            }

            if (Month < 1 || Month > 12)
            {
                Message = "Month is required.";
                return;
            }

            if (Year < 1)
            {
                Message = "Year is required.";
                return;
            }

            if (Fee < 0 ||
                Gross < 0 ||
                NetTotal < 0 ||
                EmployerContribution < 0 ||
                PD7Total < 0 ||
                Transfer < 0 ||
                PaidSoFar < 0 ||
                Received < 0 ||
                ActiveEmployees < 0)
            {
                Message = "Negative values are not allowed.";
                return;
            }

            var calculated = await _pd7LookupService.CalculatePd7Async(RpNumber, Month, Year);
            ApplyCalculatedFields(calculated);

            var dto = new PD7LookupDto
            {
                RPNumber = RpNumber,
                Fee = Fee,
                Gross = Gross,
                NetTotal = NetTotal,
                EmployerContribution = EmployerContribution,
                PD7Total = PD7Total,
                ActiveEmployees = ActiveEmployees,
                Period = PeriodSummary,
                Transfer = Transfer,
                Received = TotalReceived,
                FeeCheck = FeeCheck,
                Month = Month,
                Year = Year
            };

            var saved = await _pd7LookupService.CreatePd7Async(dto);
            if (saved)
            {
                ResetFormState();
                await LoadEmployerOptionsAsync();
                Message = "PD7 saved successfully.";
            }
            else
            {
                Message = "Unable to save PD7 record.";
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyCalculatedFields(PD7LookupDto calculated)
    {
        Fee = calculated.Fee;
        Gross = calculated.Gross;
        NetTotal = calculated.NetTotal;
        EmployerContribution = calculated.EmployerContribution;
        PD7Total = calculated.PD7Total;
        ActiveEmployees = calculated.ActiveEmployees;
        Period = calculated.Period;
        Transfer = calculated.Transfer;

        UpdateFeeCheck();
    }

    private void ApplySavedFields(PD7LookupDto saved)
    {
        PaidSoFar = saved.Received;
        Received = 0;
        UpdateFeeCheck();
    }

    private void ClearCalculatedFields()
    {
        Fee = SelectedEmployer?.Fee ?? 0;
        Gross = 0;
        NetTotal = 0;
        EmployerContribution = 0;
        PD7Total = 0;
        CalendarYearPD7Total = 0;
        ActiveEmployees = 0;
        Period = "XX";
        Transfer = 0;
        PaidSoFar = 0;
    }

    private void ResetFormState()
    {
        _suppressCalculations = true;
        _suppressSearchTextChanged = true;

        SearchText = "";
        SelectedEmployer = null;
        RpNumber = "";
        Fee = 0;
        Gross = 0;
        NetTotal = 0;
        EmployerContribution = 0;
        PD7Total = 0;
        CalendarYearPD7Total = 0;
        ActiveEmployees = 0;
        Period = "XX";
        Transfer = 0;
        PaidSoFar = 0;
        Received = 0;
        FeeCheck = false;
        Year = DateTime.Today.Year;
        SelectedMonth = Months.First(month => month.Number == DateTime.Today.Month);
        IsDropdownVisible = NeedsEmployerSelection && Employers.Count > 0;
        Message = "";

        _suppressSearchTextChanged = false;
        _suppressCalculations = false;

        UpdateFeeCheck();
    }

    private void UpdateFeeCheck()
    {
        var totalReceived = TotalReceived;
        var hasAmountToCheck = Transfer != 0 || totalReceived != 0;
        FeeCheck = hasAmountToCheck && RoundMoney(totalReceived) == RoundMoney(Transfer);
        OnPropertyChanged(nameof(TotalReceived));
        OnPropertyChanged(nameof(OutstandingBalance));
        OnPropertyChanged(nameof(HasPaidSoFar));
        OnPropertyChanged(nameof(ReceivedStatusColor));
        OnPropertyChanged(nameof(FeeCheckStatusText));
    }

    private void NotifyCalculationCheckChanged()
    {
        OnPropertyChanged(nameof(CalculationCheck));
        OnPropertyChanged(nameof(CalculationCheckDifference));
        OnPropertyChanged(nameof(CalculationCheckStatusColor));
        OnPropertyChanged(nameof(CalculationCheckStatusText));
    }

    private bool HasCalculationAmounts()
    {
        return Gross != 0 || NetTotal != 0 || EmployerContribution != 0 || PD7Total != 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static IReadOnlyList<int> BuildYearOptions()
    {
        var currentYear = DateTime.Today.Year;

        return Enumerable
            .Range(currentYear - 10, 12)
            .Reverse()
            .ToList();
    }
}

public sealed class MonthOption
{
    private MonthOption(int number)
    {
        Number = number;
        Name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(number);
    }

    public int Number { get; }
    public string Name { get; }
    public string DisplayText => $"{Number:00} - {Name}";

    public static IReadOnlyList<MonthOption> All { get; } = Enumerable
        .Range(1, 12)
        .Select(number => new MonthOption(number))
        .ToList();
}
