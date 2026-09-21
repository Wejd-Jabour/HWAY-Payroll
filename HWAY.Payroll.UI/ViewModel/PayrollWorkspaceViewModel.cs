using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using HWAY.Payroll.Services;
using HWAY.Payroll.Services.Calculations;
using HWAY.Payroll.Domain;
using HWAY.Payroll.Services.Employers;
using HWAY.Payroll.Services.PayPeriods;
using HWAY.Payroll.Services.PayrollRecord;

namespace HWAY.Payroll.UI.ViewModels;

public class PayrollWorkspaceViewModel : INotifyPropertyChanged
{
    private readonly IEmployerLookupService _employerLookupService;
    private readonly IPayrollRecordLookupService _payrollRecordService;
    private readonly IPayPeriodRangeService _payPeriodRangeService;
    private readonly IPayrollDeductionCalculator _deductionCalculator;
    private readonly IPayrollDeductionLimitService _deductionLimitService;

    private string _searchText = string.Empty;
    private EmployerLookupDto? _selectedEmployer;
    private bool _isDropdownVisible;
    private bool _isBusy;
    private bool _searchResultsFound;
    private bool _employeeNotFound;
    private bool _suppressSearchTextChanged;
    private string _message = string.Empty;

    public PayrollWorkspaceViewModel(
        IEmployerLookupService employerLookupService,
        IPayrollRecordLookupService payrollRecordService,
        IPayPeriodRangeService payPeriodRangeService,
        IPayrollDeductionCalculator deductionCalculator,
        IPayrollDeductionLimitService deductionLimitService)
    {
        _employerLookupService = employerLookupService;
        _payrollRecordService = payrollRecordService;
        _payPeriodRangeService = payPeriodRangeService;
        _deductionCalculator = deductionCalculator;
        _deductionLimitService = deductionLimitService;

        Employers = new ObservableCollection<EmployerLookupDto>();
        EmployeePayrollRows = new ObservableCollection<EmployeePayrollRowViewModel>();

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async employer => await SelectEmployerAsync(employer));
        DeselectEmployerCommand = new Command(async () => await InitializeAsync());

        CalculateRowCommand = new Command<EmployeePayrollRowViewModel>(async row => await CalculateRowAsync(row));
        SaveAllRowsCommand = new Command(async () => await SaveAllRowsAsync());
        PreviewRowPaystubCommand = new Command<EmployeePayrollRowViewModel>(PreviewRowPaystub);
        OpenCraWebsiteCommand = new Command(OpenCraWebsite);
    }

    public ObservableCollection<EmployerLookupDto> Employers { get; }

    public ObservableCollection<EmployeePayrollRowViewModel> EmployeePayrollRows { get; }

    public ICommand SearchCommand { get; }
    public ICommand SelectEmployerCommand { get; }
    public ICommand DeselectEmployerCommand { get; }

    public ICommand CalculateRowCommand { get; }
    public ICommand SaveAllRowsCommand { get; }
    public ICommand PreviewRowPaystubCommand { get; }
    public ICommand OpenCraWebsiteCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                if (!_suppressSearchTextChanged)
                {
                    _ = SearchEmployersAsync();
                }
            }
        }
    }

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

    public bool IsDropdownVisible
    {
        get => _isDropdownVisible;
        set => SetProperty(ref _isDropdownVisible, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public bool SearchResultsFound
    {
        get => _searchResultsFound;
        set => SetProperty(ref _searchResultsFound, value);
    }

    public bool EmployeeNotFound
    {
        get => _employeeNotFound;
        set => SetProperty(ref _employeeNotFound, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public async Task InitializeAsync()
    {
        if (IsBusy)
            return;

        _suppressSearchTextChanged = true;
        SearchText = string.Empty;
        _suppressSearchTextChanged = false;

        SelectedEmployer = null;
        EmployeePayrollRows.Clear();
        SearchResultsFound = false;
        EmployeeNotFound = false;
        Message = string.Empty;

        await SearchEmployersAsync();
    }

    private async Task SearchEmployersAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var results = await _employerLookupService.SearchEmployersAsync(SearchText);

            Employers.Clear();

            foreach (var employer in results)
                Employers.Add(employer);

            IsDropdownVisible = NeedsEmployerSelection && Employers.Count > 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SelectEmployerAsync(EmployerLookupDto? employer)
    {
        if (employer is null)
            return;

        try
        {
            IsBusy = true;
            IsDropdownVisible = false;

            SelectedEmployer = await _employerLookupService.GetEmployerByRpNumberAsync(employer.RpNumber);
            Message = string.Empty;

            _suppressSearchTextChanged = true;
            SearchText = employer.RpNumber;
            _suppressSearchTextChanged = false;

            await LoadEmployeesForSelectedEmployerAsync();
        }
        finally
        {
            _suppressSearchTextChanged = false;
            IsBusy = false;
        }
    }

    private async Task LoadEmployeesForSelectedEmployerAsync()
    {
        EmployeePayrollRows.Clear();

        if (SelectedEmployer is null)
        {
            SearchResultsFound = false;
            EmployeeNotFound = true;
            return;
        }

        var currentPeriod = PayPeriodRules.NormalizePeriod(
            SelectedEmployer.EmployerGroup,
            SelectedEmployer.PaystubPeriod);
        var payPeriodRange = await _payPeriodRangeService.GetPayPeriodRangeAsync(
            SelectedEmployer.EmployerGroup,
            currentPeriod);
        var results = await _payrollRecordService.SearchPayrollRecordsByRpNumberAsync(SelectedEmployer.RpNumber);

        foreach (var record in results)
        {
            EmployeePayrollRows.Add(new EmployeePayrollRowViewModel(
                SelectedEmployer,
                record,
                currentPeriod,
                payPeriodRange.Beginning,
                payPeriodRange.Ending,
                _deductionLimitService));
        }

        SearchResultsFound = EmployeePayrollRows.Count > 0;
        EmployeeNotFound = !SearchResultsFound;
    }

    private async Task CalculateRowAsync(EmployeePayrollRowViewModel? row)
    {
        if (row is null)
            return;

        try
        {
            await row.CalculatePayrollAsync(_deductionCalculator);
            Message = $"Calculated payroll deductions for {row.EmployeeName}.";
        }
        catch (Exception ex)
        {
            Message = $"Unable to calculate payroll deductions: {ex.Message}";
        }
    }

    private async Task SaveAllRowsAsync()
    {
        if (IsBusy || SelectedEmployer is null || EmployeePayrollRows.Count == 0)
            return;

        try
        {
            IsBusy = true;
            Message = string.Empty;

            var updates = EmployeePayrollRows
                .Select(row => new PayrollRecordBatchUpdateDto
                {
                    PayrollRecordId = row.Id,
                    PayrollRecord = row.ToUpdateDto()
                })
                .ToList();

            var result = await _payrollRecordService.UpdatePayrollRecordsForEmployerAsync(
                SelectedEmployer.RpNumber,
                updates);

            SelectedEmployer.PaystubPeriod = result.NextPeriod;
            OnPropertyChanged(nameof(SelectedEmployer));

            Message = BuildSaveMessage(result);

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

    private void PreviewRowPaystub(EmployeePayrollRowViewModel? row)
    {
        if (row is null)
            return;

        row.ShowPreviewPaystub();
    }

    private void OpenCraWebsite()
    {
        Launcher.OpenAsync("https://apps.cra-arc.gc.ca/ebci/rhpd/beta/entry");
    }

    private static string BuildSaveMessage(EmployerPayrollSaveResult result)
    {
        var message = $"Saved {result.UpdatedRecords} employees for period {result.SavedPeriod}. Next period is {result.NextPeriod}.";

        if (!result.PD7Month.HasValue || !result.PD7Year.HasValue)
        {
            return message;
        }

        var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(result.PD7Month.Value);
        var periodText = string.IsNullOrWhiteSpace(result.PD7Period)
            ? ""
            : $" ({result.PD7Period})";

        return $"{message} Auto-saved PD7 for {monthName} {result.PD7Year.Value}{periodText}.";
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

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class EmployeePayrollRowViewModel : INotifyPropertyChanged
{
    private readonly EmployerLookupDto _employer;
    private readonly PayrollRecordLookupDto _record;
    private readonly IPayrollDeductionLimitService _deductionLimitService;
    private PayrollDeductionLimitValues? _deductionLimits;

    private int PayrollYear => PayDate.Year;
    private PayrollDeductionLimitValues? CurrentDeductionLimits =>
        _deductionLimits ?? PayrollDeductionCaps.GetDefaultLimits(PayrollYear);

    public EmployeePayrollRowViewModel(
        EmployerLookupDto employer,
        PayrollRecordLookupDto record,
        int period,
        DateTime periodBeginning,
        DateTime periodEnding,
        IPayrollDeductionLimitService deductionLimitService)
    {
        _employer = employer;
        _record = record;
        _deductionLimitService = deductionLimitService;

        Id = record.Id;
        EmployeeNumber = record.EmployeeNumber;
        EmployeeName = record.EmployeeName;
        EmployeeEmailAddress = record.EmployeeEmailAddress;
        Period = period;
        PeriodBeginning = periodBeginning.Date;
        PeriodEnding = periodEnding.Date;

        Salary = record.Salary;
        Rate = record.Salary > 0 ? 0 : record.Rate;
        RegularHours = 0;
        OvertimeHours = 0;
        PublicHolidayHours = 0;
        StatHolidayHours = 0;
        Gross = 0;
        Vacation = 0;
        _vacationSetting = VacationSettings.Normalize(record.VacationSetting);

        VacationYtd = record.VacationYtd;
        VacationYtdUsed = record.VacationYtdUsed;
        VacationYtdBalance = record.VacationYtdBalance;

        Tax1 = 0;
        Cpp1 = 0;
        Cpp2 = 0;
        Ei = 0;
        OtherDeduction = 0;
        VacationUsed = 0;

        _ = LoadDeductionLimitsAsync(PayrollYear);
    }

    public int Id { get; }
    public string EmployeeNumber { get; }
    public string EmployeeName { get; }
    public string EmployeeEmailAddress { get; }
    public IReadOnlyList<string> VacationSettingOptions { get; } = VacationSettings.Options;
    public bool CanContributeCpp => EmployeePayrollEligibility.CanContributeCpp(_record.DateOfBirth, PayDate);
    public bool CanContributeEi => EmployeePayrollEligibility.CanContributeEi(_record.EiExempt);

    public int Period { get; private set; }
    public DateTime PeriodBeginning { get; private set; }
    public DateTime PeriodEnding { get; private set; }

    private decimal _salary;
    public decimal Salary
    {
        get => _salary;
        set
        {
            if (SetProperty(ref _salary, value) && value > 0)
            {
                SetProperty(ref _rate, 0m, nameof(Rate));
            }
        }
    }

    private decimal _rate;
    public decimal Rate
    {
        get => _rate;
        set
        {
            if (SetProperty(ref _rate, value) && value > 0)
            {
                SetProperty(ref _salary, 0m, nameof(Salary));
            }
        }
    }

    private decimal _regularHours;
    public decimal RegularHours
    {
        get => _regularHours;
        set => SetProperty(ref _regularHours, value);
    }

    private decimal _overtimeHours;
    public decimal OvertimeHours
    {
        get => _overtimeHours;
        set => SetProperty(ref _overtimeHours, value);
    }

    private decimal _publicHolidayHours;
    public decimal PublicHolidayHours
    {
        get => _publicHolidayHours;
        set => SetProperty(ref _publicHolidayHours, value);
    }

    private decimal _statHolidayHours;
    public decimal StatHolidayHours
    {
        get => _statHolidayHours;
        set => SetProperty(ref _statHolidayHours, value);
    }

    private decimal _gross;
    public decimal Gross
    {
        get => _gross;
        set => SetProperty(ref _gross, value);
    }

    private decimal _vacation;
    public decimal Vacation
    {
        get => _vacation;
        set
        {
            if (SetProperty(ref _vacation, value))
            {
                OnPropertyChanged(nameof(VacationUsed));
                RefreshVacationTotalsPreview();
            }
        }
    }

    private string _vacationSetting = VacationSettings.Accumulate;
    public string VacationSetting
    {
        get => _vacationSetting;
        set
        {
            var normalizedValue = VacationSettings.Normalize(value);

            if (SetProperty(ref _vacationSetting, normalizedValue))
            {
                OnPropertyChanged(nameof(IsVacationPaidOut));
                OnPropertyChanged(nameof(CanEditVacationUsed));
                OnPropertyChanged(nameof(VacationUsed));
                RefreshVacationTotalsPreview();
            }
        }
    }

    public bool IsVacationPaidOut => VacationSettings.IsPaidOut(VacationSetting);

    public bool CanEditVacationUsed => !IsVacationPaidOut;

    private decimal _tax1;
    public decimal Tax1
    {
        get => _tax1;
        set => SetProperty(ref _tax1, value);
    }

    private decimal _cpp1;
    public decimal Cpp1
    {
        get => _cpp1;
        set => SetDeductionWithCap(
            ref _cpp1,
            value,
            _record.Cpp1Ytd,
            PayrollDeductionCaps.ClampCpp1,
            CanContributeCpp,
            nameof(Cpp1),
            nameof(CanEditCpp1));
    }

    private decimal _cpp2;
    public decimal Cpp2
    {
        get => _cpp2;
        set => SetDeductionWithCap(
            ref _cpp2,
            value,
            _record.Cpp2Ytd,
            PayrollDeductionCaps.ClampCpp2,
            CanContributeCpp,
            nameof(Cpp2),
            nameof(CanEditCpp2));
    }

    private decimal _ei;
    public decimal Ei
    {
        get => _ei;
        set => SetDeductionWithCap(
            ref _ei,
            value,
            _record.EiYtd,
            PayrollDeductionCaps.ClampEi,
            CanContributeEi,
            nameof(Ei),
            nameof(CanEditEi));
    }

    public bool CanEditCpp1 => CanContributeCpp && (PayrollDeductionCaps.HasCpp1Room(_record.Cpp1Ytd, CurrentDeductionLimits) || _cpp1 > 0);

    public bool CanEditCpp2 => CanContributeCpp && (PayrollDeductionCaps.HasCpp2Room(_record.Cpp2Ytd, CurrentDeductionLimits) || _cpp2 > 0);

    public bool CanEditEi => CanContributeEi && (PayrollDeductionCaps.HasEiRoom(_record.EiYtd, CurrentDeductionLimits) || _ei > 0);

    private decimal _otherDeduction;
    public decimal OtherDeduction
    {
        get => _otherDeduction;
        set => SetProperty(ref _otherDeduction, value);
    }

    private bool _isPaystubVisible;
    public bool IsPaystubVisible
    {
        get => _isPaystubVisible;
        set => SetProperty(ref _isPaystubVisible, value);
    }

    private object? _previewPaystub;
    public object? PreviewPaystub
    {
        get => _previewPaystub;
        set => SetProperty(ref _previewPaystub, value);
    }

    private decimal _vacationUsed;
    public decimal VacationUsed
    {
        get => IsVacationPaidOut ? Vacation : _vacationUsed;
        set
        {
            if (SetProperty(ref _vacationUsed, value))
            {
                RefreshVacationTotalsPreview();
            }
        }
    }

    private decimal _vacationYtd;
    public decimal VacationYtd
    {
        get => _vacationYtd;
        set => SetProperty(ref _vacationYtd, value);
    }

    private decimal _vacationYtdUsed;
    public decimal VacationYtdUsed
    {
        get => _vacationYtdUsed;
        set => SetProperty(ref _vacationYtdUsed, value);
    }

    private decimal _vacationYtdBalance;
    public decimal VacationYtdBalance
    {
        get => _vacationYtdBalance;
        set => SetProperty(ref _vacationYtdBalance, value);
    }

    private DateTime _payDate = DateTime.Today;
    public DateTime PayDate
    {
        get => _payDate;
        set
        {
            var previousYear = PayrollYear;

            if (SetProperty(ref _payDate, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PayDateDisplay)));
                RefreshDeductionEligibility();

                if (previousYear != PayrollYear)
                {
                    _ = LoadDeductionLimitsAsync(PayrollYear);
                }
            }
        }
    }

    public DateTime PayDateDisplay
    {
        get => PayDate;
        set => PayDate = value;
    }


    public async Task CalculatePayrollAsync(IPayrollDeductionCalculator deductionCalculator)
    {
        Gross = CalculateGross(Salary, Rate, RegularHours, OvertimeHours, PublicHolidayHours, StatHolidayHours);

        Vacation = Gross * 0.04m;

        var deductionResult = await deductionCalculator.CalculateAsync(new PayrollDeductionCalculationRequest
        {
            PayDate = PayDate,
            PayPeriodsPerYear = PayPeriodRules.GetMaxPeriod(_employer.EmployerGroup),
            CurrentPeriod = Period,
            Province = _record.EmployeeProvince,
            GrossIncome = Gross,
            PaidVacation = IsVacationPaidOut ? Vacation : 0m,
            PensionableIncomeYtd = _record.GrossYtd + _record.VacationYtdUsed,
            Cpp1Ytd = _record.Cpp1Ytd,
            Cpp2Ytd = _record.Cpp2Ytd,
            EiYtd = _record.EiYtd,
            CanContributeCpp = CanContributeCpp,
            CanContributeEi = CanContributeEi
        });

        Tax1 = deductionResult.Tax;
        Cpp1 = deductionResult.Cpp1;
        Cpp2 = deductionResult.Cpp2;
        Ei = deductionResult.Ei;

        VacationYtd = _record.VacationYtd + Vacation;
        VacationYtdUsed = _record.VacationYtdUsed + VacationUsed;
        VacationYtdBalance = VacationYtd - VacationYtdUsed;
    }

    private void RefreshVacationTotalsPreview()
    {
        VacationYtd = _record.VacationYtd + Vacation;
        VacationYtdUsed = _record.VacationYtdUsed + VacationUsed;
        VacationYtdBalance = VacationYtd - VacationYtdUsed;
    }

    public PayrollRecordUpdateDto ToUpdateDto()
    {
        return new PayrollRecordUpdateDto
        {
            Salary = Salary,
            Rate = Rate,
            RegularHours = RegularHours,
            OvertimeHours = OvertimeHours,
            PublicHolidayHours = PublicHolidayHours,
            StatHolidayHours = StatHolidayHours,
            Gross = Gross,
            Vacation = Vacation,
            VacationSetting = VacationSetting,
            VacationUsed = VacationUsed,
            Tax1 = Tax1,
            Cpp1 = Cpp1,
            Cpp2 = Cpp2,
            Ei = Ei,
            OtherDeduction = OtherDeduction,
            PayDate = PayDate
        };
    }

    public void ShowPreviewPaystub()
    {
        PreviewPaystub = new PaystubTemplateBindingModel(
            _employer,
            _record,
            Salary,
            Rate,
            RegularHours,
            OvertimeHours,
            PublicHolidayHours,
            StatHolidayHours,
            Gross,
            Vacation,
            Tax1,
            Cpp1,
            Cpp2,
            Ei,
            OtherDeduction,
            VacationUsed,
            VacationSetting,
            Period,
            PeriodBeginning,
            PeriodEnding,
            PayDate);

        IsPaystubVisible = true;
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

    private void SetDeductionWithCap(
        ref decimal field,
        decimal requestedValue,
        decimal currentYtd,
        Func<decimal, decimal, PayrollDeductionLimitValues?, decimal> capDeduction,
        bool canContributeDeduction,
        string propertyName,
        string canEditPropertyName)
    {
        var cappedValue = canContributeDeduction
            ? capDeduction(requestedValue, currentYtd, CurrentDeductionLimits)
            : 0m;
        var valueChanged = !EqualityComparer<decimal>.Default.Equals(field, cappedValue);

        if (!valueChanged && requestedValue == cappedValue)
            return;

        field = cappedValue;
        OnPropertyChanged(propertyName);
        OnPropertyChanged(canEditPropertyName);
    }

    private async Task LoadDeductionLimitsAsync(int year)
    {
        try
        {
            _deductionLimits = await _deductionLimitService.GetLimitValuesForYearAsync(year);
        }
        catch
        {
            _deductionLimits = PayrollDeductionCaps.GetDefaultLimits(year);
        }

        OnPropertyChanged(nameof(CanEditCpp1));
        OnPropertyChanged(nameof(CanEditCpp2));
        OnPropertyChanged(nameof(CanEditEi));
    }

    private void RefreshDeductionEligibility()
    {
        if (!CanContributeCpp)
        {
            SetProperty(ref _cpp1, 0m, nameof(Cpp1));
            SetProperty(ref _cpp2, 0m, nameof(Cpp2));
        }

        if (!CanContributeEi)
        {
            SetProperty(ref _ei, 0m, nameof(Ei));
        }

        OnPropertyChanged(nameof(CanContributeCpp));
        OnPropertyChanged(nameof(CanContributeEi));
        OnPropertyChanged(nameof(CanEditCpp1));
        OnPropertyChanged(nameof(CanEditCpp2));
        OnPropertyChanged(nameof(CanEditEi));
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static decimal CalculateGross(
        decimal salary,
        decimal rate,
        decimal regularHours,
        decimal overtimeHours,
        decimal publicHolidayHours,
        decimal statHolidayHours)
    {
        var regularPay = rate * regularHours;
        var overtimePay = rate * 1.5m * overtimeHours;
        var publicHolidayPay = rate * publicHolidayHours;
        var statHolidayPay = rate * 1.5m * statHolidayHours;

        return salary + regularPay + overtimePay + publicHolidayPay + statHolidayPay;
    }
}

public class PaystubTemplateBindingModel
{
    public PaystubTemplateBindingModel(
    EmployerLookupDto employer,
    PayrollRecordLookupDto record,
    decimal salary,
    decimal rate,
    decimal regularHours,
    decimal overtimeHours,
    decimal publicHolidayHours,
    decimal statHolidayHours,
    decimal gross,
    decimal vacation,
    decimal tax1,
    decimal cpp1,
    decimal cpp2,
    decimal ei,
    decimal otherDeduction,
    decimal vacationUsed,
    string vacationSetting,
    int period,
    DateTime periodBeginning,
    DateTime periodEnding,
    DateTime payDate)
    {
        EmployerName = employer.EmployerName;
        Address = employer.Address ?? "";
        City = employer.City ?? "";
        PostalCode = employer.PostalCode ?? "";
        EmployerGroup = employer.EmployerGroup ?? "";

        EmployeeNumber = record.EmployeeNumber;
        EmployeeName = record.EmployeeName;

        EmployeeAddressLine = record.EmployeeAddress;
        EmployeeCityProvinceLine = BuildCityProvinceLine(
            record.EmployeeCity,
            record.EmployeeProvince,
            record.EmployeePostalCode);

        EmployeeEmailLine = record.EmployeeEmailAddress;

        PayDate = payDate.ToString("yyyy-MM-dd");
        Period = period.ToString();
        PayPeriod = period.ToString();
        PeriodBeginning = periodBeginning.ToString("yyyy-MM-dd");
        PeriodEnding = periodEnding.ToString("yyyy-MM-dd");

        var regularPay = salary + rate * regularHours;
        var overtimePay = rate * 1.5m * overtimeHours;
        var publicHolidayPay = rate * publicHolidayHours;
        var statHolidayPay = rate * 1.5m * statHolidayHours;
        var normalizedVacationSetting = VacationSettings.Normalize(vacationSetting);
        var isVacationPaidOut = VacationSettings.IsPaidOut(normalizedVacationSetting);
        var paidVacation = isVacationPaidOut ? vacation : 0m;
        var currentVacationUsed = isVacationPaidOut ? vacation : vacationUsed;
        var grossYtd = record.GrossYtd + gross;
        var taxYtd = record.TaxYtd + tax1;
        var cpp1Ytd = record.Cpp1Ytd + cpp1;
        var cpp2Ytd = record.Cpp2Ytd + cpp2;
        var eiYtd = record.EiYtd + ei;
        var otherDeductionYtd = record.OtherDeductionYtd + otherDeduction;
        var vacationYtd = record.VacationYtd + vacation;
        var vacationYtdUsed = record.VacationYtdUsed + currentVacationUsed;
        var vacationYtdBalance = vacationYtd - vacationYtdUsed;

        RegularPaySalary = FormatMoney(salary);
        RegularPayHours = FormatNumber(regularHours);
        RegularPayRate = FormatMoney(rate);
        RegularPayCurrent = FormatMoney(regularPay);
        RegularPayYtd = FormatMoney(grossYtd);

        OvertimePaySalary = "";
        OvertimePayHours = FormatNumber(overtimeHours);
        OvertimePayRate = FormatMoneyWhenHoursSet(rate * 1.5m, overtimeHours);
        OvertimePayCurrent = FormatMoney(overtimePay);
        OvertimePayYtd = "";

        PublicHolidaySalary = "";
        PublicHolidayHours = FormatNumber(publicHolidayHours);
        PublicHolidayRate = FormatMoneyWhenHoursSet(rate, publicHolidayHours);
        PublicHolidayCurrent = FormatMoney(publicHolidayPay);
        PublicHolidayYtd = "";

        StatHolidaySalary = "";
        StatHolidayHours = FormatNumber(statHolidayHours);
        StatHolidayRate = FormatMoneyWhenHoursSet(rate * 1.5m, statHolidayHours);
        StatHolidayCurrent = FormatMoney(statHolidayPay);
        StatHolidayYtd = "";

        ShowVacationPay = isVacationPaidOut;
        VacationPayCurrent = isVacationPaidOut ? FormatMoney(vacation) : "";
        VacationPayYtd = isVacationPaidOut ? FormatMoney(vacationYtdUsed) : "";

        TotalPayCurrent = FormatMoney(gross + paidVacation);
        TotalPayYtd = FormatMoney(grossYtd + vacationYtdUsed);

        TaxCurrent = FormatMoney(tax1);
        TaxYtd = FormatMoney(taxYtd);

        Cpp1Current = FormatMoney(cpp1);
        Cpp1Ytd = FormatMoney(cpp1Ytd);

        Cpp2Current = FormatMoney(cpp2);
        Cpp2Ytd = FormatMoney(cpp2Ytd);

        EiCurrent = FormatMoney(ei);
        EiYtd = FormatMoney(eiYtd);

        OtherDeductionCurrent = FormatMoney(otherDeduction);
        OtherDeductionYtd = FormatMoney(otherDeductionYtd);

        var totalPay = gross + paidVacation;
        var netPay = CalculateNetPay(gross, vacation, normalizedVacationSetting, tax1, cpp1, cpp2, ei, otherDeduction);
        var netYtd = record.NetYtd + netPay;

        HeaderNetPay = FormatMoney(netPay);
        BenefitNetPay = FormatMoney(netPay);

        SummaryTotalPayCurrent = FormatMoney(totalPay);
        SummaryTotalPayYtd = FormatMoney(grossYtd + vacationYtdUsed);

        SummaryTaxesCurrent = FormatMoney(tax1);
        SummaryTaxesYtd = FormatMoney(taxYtd);

        SummaryDeductionsCurrent = FormatMoney(cpp1 + cpp2 + ei + otherDeduction);
        SummaryDeductionsYtd = FormatMoney(
            cpp1Ytd +
            cpp2Ytd +
            eiYtd +
            otherDeductionYtd);

        SummaryNetPayCurrent = FormatMoney(netPay);
        SummaryNetPayYtd = FormatMoney(netYtd);

        VacationYtdBalance = FormatMoney(vacationYtdBalance);
        Memo = "";
    }

    public string EmployerGroup { get; }
    public string EmployerName { get; }
    public string Address { get; }
    public string City { get; }
    public string PostalCode { get; }

    public string EmployeeNumber { get; }
    public string EmployeeName { get; }
    public string EmployeeAddressLine { get; }
    public string EmployeeCityProvinceLine { get; }
    public string EmployeeEmailLine { get; }

    public string PayDate { get; }
    public string Period { get; }
    public string PayPeriod { get; }
    public string PeriodBeginning { get; }
    public string PeriodEnding { get; }

    public string HeaderNetPay { get; }
    public string BenefitNetPay { get; }
    public string VacationYtdBalance { get; }
    public string Memo { get; }

    public string RegularPaySalary { get; }
    public string RegularPayHours { get; }
    public string RegularPayRate { get; }
    public string RegularPayCurrent { get; }
    public string RegularPayYtd { get; }

    public string OvertimePaySalary { get; }
    public string OvertimePayHours { get; }
    public string OvertimePayRate { get; }
    public string OvertimePayCurrent { get; }
    public string OvertimePayYtd { get; }

    public string PublicHolidaySalary { get; }
    public string PublicHolidayHours { get; }
    public string PublicHolidayRate { get; }
    public string PublicHolidayCurrent { get; }
    public string PublicHolidayYtd { get; }

    public string StatHolidaySalary { get; }
    public string StatHolidayHours { get; }
    public string StatHolidayRate { get; }
    public string StatHolidayCurrent { get; }
    public string StatHolidayYtd { get; }

    public bool ShowVacationPay { get; }
    public string VacationPayCurrent { get; }
    public string VacationPayYtd { get; }
    public string TotalPayCurrent { get; }
    public string TotalPayYtd { get; }

    public string TaxCurrent { get; }
    public string TaxYtd { get; }
    public string Cpp1Current { get; }
    public string Cpp1Ytd { get; }
    public string Cpp2Current { get; }
    public string Cpp2Ytd { get; }
    public string EiCurrent { get; }
    public string EiYtd { get; }
    public string OtherDeductionCurrent { get; }
    public string OtherDeductionYtd { get; }

    public string SummaryTotalPayCurrent { get; }
    public string SummaryTotalPayYtd { get; }
    public string SummaryTaxesCurrent { get; }
    public string SummaryTaxesYtd { get; }
    public string SummaryDeductionsCurrent { get; }
    public string SummaryDeductionsYtd { get; }
    public string SummaryNetPayCurrent { get; }
    public string SummaryNetPayYtd { get; }

    private static decimal CalculateNetPay(
        decimal gross,
        decimal vacation,
        string vacationSetting,
        decimal tax1,
        decimal cpp1,
        decimal cpp2,
        decimal ei,
        decimal otherDeduction)
    {
        var totalDeductions = tax1 + cpp1 + cpp2 + ei + otherDeduction;
        var paidVacation = VacationSettings.IsPaidOut(vacationSetting) ? vacation : 0m;

        return gross + paidVacation - totalDeductions;
    }

    private static string FormatMoney(decimal value) => value == 0 ? "" : value.ToString("0.00");

    private static string FormatNumber(decimal value) => value == 0 ? "" : value.ToString("0.##");

    private static string FormatMoneyWhenHoursSet(decimal value, decimal hours) =>
        hours == 0 ? "" : FormatMoney(value);

    private static string BuildCityProvinceLine(string city, string province, string postalCode)
    {
        var cityProvince = string.Join(", ",
            new[] { city, province }.Where(value => !string.IsNullOrWhiteSpace(value)));

        return string.Join(" ",
            new[] { cityProvince, postalCode }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }
}
