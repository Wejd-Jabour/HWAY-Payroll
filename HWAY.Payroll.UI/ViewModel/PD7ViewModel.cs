using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.Employers;
using HWAY.Payroll.Services.PD7;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace HWAY.Payroll.UI.ViewModel;

public class PD7ViewModel : INotifyPropertyChanged
{
    private readonly IPD7LookupService _pd7LookupService;
    private readonly IEmployerLookupService _employerLookupService;
    private bool _suppressSearchTextChanged;
    private bool _suppressCalculations;
    private bool _isCalculating;

    public PD7ViewModel(
        IPD7LookupService pd7LookupService,
        IEmployerLookupService employerLookupService)
    {
        _pd7LookupService = pd7LookupService;
        _employerLookupService = employerLookupService;

        Employers = new ObservableCollection<EmployerLookupDto>();
        Months = MonthOption.All;

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async employer => await SelectEmployerAsync(employer));
        SaveCommand = new Command(async () => await SaveAsync());
        ClearCommand = new Command(ClearForm);

        _suppressCalculations = true;
        Period = "XX";
        Year = DateTime.Today.Year;
        SelectedMonth = Months.First(month => month.Number == DateTime.Today.Month);
        _suppressCalculations = false;

        _ = LoadInitialEmployersAsync();
    }

    public ObservableCollection<EmployerLookupDto> Employers { get; }
    public IReadOnlyList<MonthOption> Months { get; }

    public ICommand SearchCommand { get; }
    public ICommand SelectEmployerCommand { get; }
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
        set => SetProperty(ref _selectedEmployer, value);
    }

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
        set => SetProperty(ref _gross, value);
    }

    private decimal _pd7Total;
    public decimal PD7Total
    {
        get => _pd7Total;
        set => SetProperty(ref _pd7Total, value);
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

    private bool _feeCheck;
    public bool FeeCheck
    {
        get => _feeCheck;
        set => SetProperty(ref _feeCheck, value);
    }

    public Color ReceivedStatusColor
    {
        get
        {
            if (Transfer == 0 && Received == 0)
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
            if (Transfer == 0 && Received == 0)
                return "";

            return FeeCheck
                ? "Received matches transfer"
                : "Received does not match transfer";
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

    private async Task LoadInitialEmployersAsync()
    {
        await SearchEmployersAsync();
    }

    private async Task SearchEmployersAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            Message = "";

            var results = await _employerLookupService.SearchEmployersAsync(SearchText);

            Employers.Clear();
            foreach (var employer in results)
            {
                Employers.Add(employer);
            }

            IsDropdownVisible = Employers.Count > 0;
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

            var saved = await _pd7LookupService.GetByPeriodAsync(RpNumber, Month, Year);
            if (saved != null)
            {
                ApplySavedFields(saved);
                return;
            }

            var calculated = await _pd7LookupService.CalculatePd7Async(RpNumber, Month, Year);
            ApplyCalculatedFields(calculated);
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

            if (Fee < 0 || Gross < 0 || PD7Total < 0 || Transfer < 0 || Received < 0 || ActiveEmployees < 0)
            {
                Message = "Negative values are not allowed.";
                return;
            }

            var savedPd7 = await _pd7LookupService.GetByPeriodAsync(RpNumber, Month, Year);
            if (savedPd7 == null)
            {
                var calculated = await _pd7LookupService.CalculatePd7Async(RpNumber, Month, Year);
                ApplyCalculatedFields(calculated);
            }

            var dto = new PD7LookupDto
            {
                RPNumber = RpNumber,
                Fee = Fee,
                Gross = Gross,
                PD7Total = PD7Total,
                ActiveEmployees = ActiveEmployees,
                Period = PeriodSummary,
                Transfer = Transfer,
                Received = Received,
                FeeCheck = FeeCheck,
                Month = Month,
                Year = Year
            };

            var saved = await _pd7LookupService.CreatePd7Async(dto);
            if (saved)
            {
                ClearForm();
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
        PD7Total = calculated.PD7Total;
        ActiveEmployees = calculated.ActiveEmployees;
        Period = calculated.Period;
        Transfer = calculated.Transfer;

        UpdateFeeCheck();
    }

    private void ApplySavedFields(PD7LookupDto saved)
    {
        Fee = saved.Fee;
        Gross = saved.Gross;
        PD7Total = saved.PD7Total;
        ActiveEmployees = saved.ActiveEmployees;
        Period = saved.Period;
        Transfer = saved.Transfer;
        Received = saved.Received;
        FeeCheck = saved.FeeCheck;

        OnPropertyChanged(nameof(ReceivedStatusColor));
        OnPropertyChanged(nameof(FeeCheckStatusText));
    }

    private void ClearCalculatedFields()
    {
        Fee = SelectedEmployer?.Fee ?? 0;
        Gross = 0;
        PD7Total = 0;
        ActiveEmployees = 0;
        Period = "XX";
        Transfer = 0;
    }

    private void ClearForm()
    {
        _suppressCalculations = true;
        _suppressSearchTextChanged = true;

        SearchText = "";
        SelectedEmployer = null;
        RpNumber = "";
        Fee = 0;
        Gross = 0;
        PD7Total = 0;
        ActiveEmployees = 0;
        Period = "XX";
        Transfer = 0;
        Received = 0;
        FeeCheck = false;
        Year = DateTime.Today.Year;
        SelectedMonth = Months.First(month => month.Number == DateTime.Today.Month);
        IsDropdownVisible = Employers.Count > 0;
        Message = "";

        _suppressSearchTextChanged = false;
        _suppressCalculations = false;

        UpdateFeeCheck();
        _ = SearchEmployersAsync();
    }

    private void UpdateFeeCheck()
    {
        var hasAmountToCheck = Transfer != 0 || Received != 0;
        FeeCheck = hasAmountToCheck && RoundMoney(Received) == RoundMoney(Transfer);
        OnPropertyChanged(nameof(ReceivedStatusColor));
        OnPropertyChanged(nameof(FeeCheckStatusText));
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
