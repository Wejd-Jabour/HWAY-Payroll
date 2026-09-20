using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using HWAY.Payroll.Domain;
using HWAY.Payroll.Services;
using HWAY.Payroll.Services.Calculations;
using HWAY.Payroll.Services.Employers;
using HWAY.Payroll.Services.PayPeriods;
using HWAY.Payroll.Services.PayrollRecord;
using HWAY.Payroll.Services.PayrollRecordEdit;

namespace HWAY.Payroll.UI.ViewModels;

public class PayrollRecordEditViewModel : INotifyPropertyChanged
{
    private readonly IPayrollRecordEditService _service;
    private readonly IPayrollRecordLookupService _lookupService;
    private readonly IEmployerLookupService _employerLookupService;
    private readonly IPayrollDeductionCalculator _deductionCalculator;
    private readonly IPayrollDeductionLimitService _deductionLimitService;
    private readonly IPayPeriodRangeService _payPeriodRangeService;

    private int _currentPayrollRecordId;
    private decimal _grossYtdBeforeCurrent;
    private decimal _vacationYtdUsedBeforeCurrent;
    private decimal _cpp1YtdBeforeCurrent;
    private decimal _cpp2YtdBeforeCurrent;
    private decimal _eiYtdBeforeCurrent;
    private DateTime? _dateOfBirth;
    private bool _eiExempt;
    private bool _requiresRecalculate;
    private PayrollDeductionLimitValues? _deductionLimits;
    private bool _suppressSearchTextChanged;
    private bool _isLoadingEmployers;

    private static int PayrollYear => DateTime.Today.Year;
    private PayrollDeductionLimitValues? CurrentDeductionLimits =>
        _deductionLimits ?? PayrollDeductionCaps.GetDefaultLimits(PayrollYear);

    public PayrollRecordEditViewModel(
        IPayrollRecordEditService service,
        IPayrollRecordLookupService lookupService,
        IEmployerLookupService employerLookupService,
        IPayrollDeductionCalculator deductionCalculator,
        IPayrollDeductionLimitService deductionLimitService,
        IPayPeriodRangeService payPeriodRangeService)
    {
        _service = service;
        _lookupService = lookupService;
        _employerLookupService = employerLookupService;
        _deductionCalculator = deductionCalculator;
        _deductionLimitService = deductionLimitService;
        _payPeriodRangeService = payPeriodRangeService;

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async employer => await SelectEmployerAsync(employer));
        SaveCommand = new Command(async () => await SaveChanges());
        CalculatePayrollCommand = new Command(async () => await CalculatePayrollAsync());
        OpenCraWebsiteCommand = new Command(OpenCraWebsite);

        Employers = new ObservableCollection<EmployerLookupDto>();
        SearchResults = new ObservableCollection<PayrollRecordLookupDto>();
    }

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

    private string _employeeNumber = string.Empty;
    public string EmployeeNumber
    {
        get => _employeeNumber;
        set => SetProperty(ref _employeeNumber, value);
    }

    private string _rpNumber = string.Empty;
    public string RpNumber
    {
        get => _rpNumber;
        set => SetProperty(ref _rpNumber, value);
    }

    private string _employeeName = string.Empty;
    public string EmployeeName
    {
        get => _employeeName;
        set => SetProperty(ref _employeeName, value);
    }

    private string _employerName = string.Empty;
    public string EmployerName
    {
        get => _employerName;
        set => SetProperty(ref _employerName, value);
    }

    private decimal _salary;
    public decimal Salary
    {
        get => _salary;
        set
        {
            if (SetProperty(ref _salary, value))
            {
                if (value > 0)
                {
                    SetProperty(ref _rate, 0m, nameof(Rate));
                }

                _requiresRecalculate = true;
            }
        }
    }

    private decimal _rate;
    public decimal Rate
    {
        get => _rate;
        set
        {
            if (SetProperty(ref _rate, value))
            {
                if (value > 0)
                {
                    SetProperty(ref _salary, 0m, nameof(Salary));
                }

                _requiresRecalculate = true;
            }
        }
    }

    private decimal _regularHours;
    public decimal RegularHours
    {
        get => _regularHours;
        set
        {
            if (SetProperty(ref _regularHours, value))
                _requiresRecalculate = true;
        }
    }

    private decimal _overtimeHours;
    public decimal OvertimeHours
    {
        get => _overtimeHours;
        set
        {
            if (SetProperty(ref _overtimeHours, value))
                _requiresRecalculate = true;
        }
    }

    private decimal _publicHolidayHours;
    public decimal PublicHolidayHours
    {
        get => _publicHolidayHours;
        set
        {
            if (SetProperty(ref _publicHolidayHours, value))
                _requiresRecalculate = true;
        }
    }

    private decimal _statHolidayHours;
    public decimal StatHolidayHours
    {
        get => _statHolidayHours;
        set
        {
            if (SetProperty(ref _statHolidayHours, value))
                _requiresRecalculate = true;
        }
    }

    private decimal _gross;
    public decimal Gross
    {
        get => _gross;
        set
        {
            if (SetProperty(ref _gross, value))
            {
                RefreshNetPay();
            }
        }
    }

    private decimal _vacation;
    public decimal Vacation
    {
        get => _vacation;
        set
        {
            if (SetProperty(ref _vacation, value))
            {
                RefreshNetPay();
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
                NetPay = CalculateNetPay(Gross, Vacation, VacationSetting, Tax1, Cpp1, Cpp2, Ei, OtherDeduction);
            }
        }
    }

    private decimal _tax1;
    public decimal Tax1
    {
        get => _tax1;
        set
        {
            if (SetProperty(ref _tax1, value))
            {
                RefreshNetPay();
            }
        }
    }

    private decimal _cpp1;
    public decimal Cpp1
    {
        get => _cpp1;
        set => SetDeductionWithCap(
            ref _cpp1,
            value,
            _cpp1YtdBeforeCurrent,
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
            _cpp2YtdBeforeCurrent,
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
            _eiYtdBeforeCurrent,
            PayrollDeductionCaps.ClampEi,
            CanContributeEi,
            nameof(Ei),
            nameof(CanEditEi));
    }

    public bool CanContributeCpp => EmployeePayrollEligibility.CanContributeCpp(_dateOfBirth, DateTime.Today);

    public bool CanContributeEi => EmployeePayrollEligibility.CanContributeEi(_eiExempt);

    public bool CanEditCpp1 => CanContributeCpp && (PayrollDeductionCaps.HasCpp1Room(_cpp1YtdBeforeCurrent, CurrentDeductionLimits) || _cpp1 > 0);

    public bool CanEditCpp2 => CanContributeCpp && (PayrollDeductionCaps.HasCpp2Room(_cpp2YtdBeforeCurrent, CurrentDeductionLimits) || _cpp2 > 0);

    public bool CanEditEi => CanContributeEi && (PayrollDeductionCaps.HasEiRoom(_eiYtdBeforeCurrent, CurrentDeductionLimits) || _ei > 0);

    private decimal _otherDeduction;
    public decimal OtherDeduction
    {
        get => _otherDeduction;
        set
        {
            if (SetProperty(ref _otherDeduction, value))
            {
                RefreshNetPay();
            }
        }
    }

    private decimal _netPay;
    public decimal NetPay
    {
        get => _netPay;
        set => SetProperty(ref _netPay, value);
    }

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private bool _searchResultsFound;
    public bool SearchResultsFound
    {
        get => _searchResultsFound;
        set => SetProperty(ref _searchResultsFound, value);
    }

    private bool _employeeFound;
    public bool EmployeeFound
    {
        get => _employeeFound;
        set => SetProperty(ref _employeeFound, value);
    }

    private bool _employeeNotFound;
    public bool EmployeeNotFound
    {
        get => _employeeNotFound;
        set => SetProperty(ref _employeeNotFound, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand SelectEmployerCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CalculatePayrollCommand { get; }
    public ICommand OpenCraWebsiteCommand { get; }

    public ObservableCollection<EmployerLookupDto> Employers { get; }
    public ObservableCollection<PayrollRecordLookupDto> SearchResults { get; }
    public IReadOnlyList<string> VacationSettingOptions { get; } = VacationSettings.Options;

    public PayrollRecordLookupDto? SelectedPayrollRecord
    {
        get => _selectedPayrollRecord;
        set
        {
            if (SetProperty(ref _selectedPayrollRecord, value))
            {
                LoadSelectedPayrollRecord(value);
            }
        }
    }

    private PayrollRecordLookupDto? _selectedPayrollRecord;

    public async Task InitializeAsync()
    {
        if (_isLoadingEmployers)
            return;

        _suppressSearchTextChanged = true;

        SearchText = string.Empty;
        SelectedEmployer = null;
        IsDropdownVisible = false;
        ClearSelectedPayrollRecord();
        SearchResults.Clear();
        SearchResultsFound = false;
        EmployeeFound = false;
        EmployeeNotFound = false;
        Message = string.Empty;

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
            Message = string.Empty;

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
            Message = $"Error loading companies: {ex.Message}";
            IsDropdownVisible = false;
        }
        finally
        {
            _isLoadingEmployers = false;
        }
    }

    private async Task SelectEmployerAsync(EmployerLookupDto? employer)
    {
        if (employer == null || _isLoadingEmployers)
            return;

        try
        {
            Message = string.Empty;
            IsDropdownVisible = false;

            SelectedEmployer = await _employerLookupService.GetEmployerByRpNumberAsync(employer.RpNumber) ?? employer;

            _suppressSearchTextChanged = true;
            SearchText = SelectedEmployer.RpNumber;
            _suppressSearchTextChanged = false;

            await LoadEmployeesForSelectedCompanyAsync();
        }
        catch (Exception ex)
        {
            Message = $"Error selecting company: {ex.Message}";
        }
        finally
        {
            _suppressSearchTextChanged = false;
        }
    }

    private async Task LoadEmployeesForSelectedCompanyAsync()
    {
        SearchResults.Clear();
        SelectedPayrollRecord = null;
        _currentPayrollRecordId = 0;
        EmployeeFound = false;

        if (SelectedEmployer == null)
        {
            SearchResultsFound = false;
            EmployeeNotFound = false;
            return;
        }

        var results = await _lookupService.SearchPayrollRecordsByRpNumberAsync(SelectedEmployer.RpNumber);

        foreach (var result in results)
        {
            SearchResults.Add(result);
        }

        SearchResultsFound = SearchResults.Count > 0;
        EmployeeNotFound = !SearchResultsFound;
        Message = SearchResultsFound ? string.Empty : "No employees found for this company.";
    }

    private void ClearSelectedPayrollRecord()
    {
        SelectedPayrollRecord = null;
        _currentPayrollRecordId = 0;
        EmployeeNumber = string.Empty;
        RpNumber = string.Empty;
        EmployeeName = string.Empty;
        EmployerName = string.Empty;
        Salary = 0;
        Rate = 0;
        RegularHours = 0;
        OvertimeHours = 0;
        PublicHolidayHours = 0;
        StatHolidayHours = 0;
        Gross = 0;
        Vacation = 0;
        VacationSetting = VacationSettings.Accumulate;
        Tax1 = 0;
        Cpp1 = 0;
        Cpp2 = 0;
        Ei = 0;
        OtherDeduction = 0;
        NetPay = 0;
        _grossYtdBeforeCurrent = 0;
        _vacationYtdUsedBeforeCurrent = 0;
        _cpp1YtdBeforeCurrent = 0;
        _cpp2YtdBeforeCurrent = 0;
        _eiYtdBeforeCurrent = 0;
        _dateOfBirth = null;
        _eiExempt = false;
        _requiresRecalculate = false;
        OnPropertyChanged(nameof(CanContributeCpp));
        OnPropertyChanged(nameof(CanContributeEi));
        OnPropertyChanged(nameof(CanEditCpp1));
        OnPropertyChanged(nameof(CanEditCpp2));
        OnPropertyChanged(nameof(CanEditEi));
    }

    private void LoadSelectedPayrollRecord(PayrollRecordLookupDto? selectedRecord)
    {
        if (selectedRecord == null)
            return;

        _currentPayrollRecordId = selectedRecord.Id;
        EmployeeNumber = selectedRecord.EmployeeNumber;
        RpNumber = selectedRecord.RpNumber;
        EmployeeName = selectedRecord.EmployeeName;
        EmployerName = selectedRecord.EmployerName;

        Salary = selectedRecord.Salary;
        Rate = selectedRecord.Salary > 0 ? 0 : selectedRecord.Rate;
        RegularHours = selectedRecord.RegularHours;
        OvertimeHours = selectedRecord.OvertimeHours;
        PublicHolidayHours = selectedRecord.PublicHolidayHours;
        StatHolidayHours = selectedRecord.StatHolidayHours;

        Gross = selectedRecord.Gross;
        Vacation = selectedRecord.Vacation;
        VacationSetting = selectedRecord.VacationSetting;
        _requiresRecalculate = false;

        _grossYtdBeforeCurrent = Math.Max(selectedRecord.GrossYtd - selectedRecord.Gross, 0m);
        _vacationYtdUsedBeforeCurrent = Math.Max(
            selectedRecord.VacationYtdUsed - (VacationSettings.IsPaidOut(selectedRecord.VacationSetting) ? selectedRecord.Vacation : 0m),
            0m);
        _cpp1YtdBeforeCurrent = Math.Max(selectedRecord.Cpp1Ytd - selectedRecord.Cpp1, 0m);
        _cpp2YtdBeforeCurrent = Math.Max(selectedRecord.Cpp2Ytd - selectedRecord.Cpp2, 0m);
        _eiYtdBeforeCurrent = Math.Max(selectedRecord.EiYtd - selectedRecord.Ei, 0m);
        _dateOfBirth = selectedRecord.DateOfBirth;
        _eiExempt = selectedRecord.EiExempt;

        Tax1 = selectedRecord.Tax1;
        Cpp1 = selectedRecord.Cpp1;
        Cpp2 = selectedRecord.Cpp2;
        Ei = selectedRecord.Ei;
        OtherDeduction = selectedRecord.OtherDeduction;

        NetPay = selectedRecord.NetPay;

        EmployeeFound = true;
        EmployeeNotFound = false;
        OnPropertyChanged(nameof(CanContributeCpp));
        OnPropertyChanged(nameof(CanContributeEi));
        OnPropertyChanged(nameof(CanEditCpp1));
        OnPropertyChanged(nameof(CanEditCpp2));
        OnPropertyChanged(nameof(CanEditEi));
    }

    private async Task SaveChanges()
    {
        if (_currentPayrollRecordId <= 0)
        {
            Message = "Select a payroll record before saving.";
            return;
        }

        if (_requiresRecalculate)
        {
            Message = "Recalculate payroll deductions before saving.";
            return;
        }

        try
        {
            var dto = new PayrollRecordEditDto
            {
                Id = _currentPayrollRecordId,
                EmployeeNumber = EmployeeNumber,
                RpNumber = RpNumber,
                EmployeeName = EmployeeName,
                Salary = Salary,
                Rate = Rate,
                RegularHours = RegularHours,
                OvertimeHours = OvertimeHours,
                PublicHolidayHours = PublicHolidayHours,
                StatHolidayHours = StatHolidayHours,
                Gross = Gross,
                Vacation = Vacation,
                VacationSetting = VacationSetting,
                Tax1 = Tax1,
                Cpp1 = Cpp1,
                Cpp2 = Cpp2,
                Ei = Ei,
                OtherDeduction = OtherDeduction,
                NetPay = NetPay
            };

            var result = await _service.UpdatePayrollRecordAsync(_currentPayrollRecordId, dto);

            var saveMessage = BuildSaveMessage(result);

            var updatedPayrollRecordId = _currentPayrollRecordId;
            await LoadEmployeesForSelectedCompanyAsync();
            SelectedPayrollRecord = SearchResults.FirstOrDefault(record => record.Id == updatedPayrollRecordId);
            Message = saveMessage;
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    private async Task CalculatePayrollAsync()
    {
        if (SelectedPayrollRecord is null)
        {
            Message = "Select a payroll record before calculating.";
            return;
        }

        Gross = Salary +
            (Rate * RegularHours) +
            (Rate * 1.5m * OvertimeHours) +
            (Rate * PublicHolidayHours) +
            (Rate * 1.5m * StatHolidayHours);
        Vacation = Gross * 0.04m;

        try
        {
            await LoadDeductionLimitsAsync(PayrollYear);

            var deductions = await _deductionCalculator.CalculateAsync(new PayrollDeductionCalculationRequest
            {
                PayDate = DateTime.Today,
                PayPeriodsPerYear = PayPeriodRules.GetMaxPeriod(SelectedPayrollRecord.EmployerGroup),
                CurrentPeriod = PayPeriodRules.NormalizePeriod(
                    SelectedPayrollRecord.EmployerGroup,
                    SelectedPayrollRecord.EmployerPaystubPeriod),
                Province = SelectedPayrollRecord.EmployeeProvince,
                GrossIncome = Gross,
                PaidVacation = VacationSettings.IsPaidOut(VacationSetting) ? Vacation : 0m,
                PensionableIncomeYtd = _grossYtdBeforeCurrent + _vacationYtdUsedBeforeCurrent,
                Cpp1Ytd = _cpp1YtdBeforeCurrent,
                Cpp2Ytd = _cpp2YtdBeforeCurrent,
                EiYtd = _eiYtdBeforeCurrent,
                CanContributeCpp = CanContributeCpp,
                CanContributeEi = CanContributeEi
            });

            Tax1 = deductions.Tax;
            Cpp1 = deductions.Cpp1;
            Cpp2 = deductions.Cpp2;
            Ei = deductions.Ei;
        }
        catch (Exception ex)
        {
            Message = $"Unable to calculate payroll deductions: {ex.Message}";
            return;
        }

        NetPay = CalculateNetPay(Gross, Vacation, VacationSetting, Tax1, Cpp1, Cpp2, Ei, OtherDeduction);
        _requiresRecalculate = false;
        Message = "Payroll deductions recalculated.";
    }

    private static string BuildSaveMessage(PayrollRecordEditResultDto result)
    {
        var message = "Payroll record corrected successfully.";

        if (result.HistoryUpdated)
        {
            message += " Latest payroll history snapshot updated.";
        }

        if (result.Pd7Updated && result.Pd7Month.HasValue && result.Pd7Year.HasValue)
        {
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(result.Pd7Month.Value);
            message += $" Existing PD7 recalculated for {monthName} {result.Pd7Year.Value}.";
        }
        else if (result.HistoryUpdated && result.Pd7Month.HasValue && result.Pd7Year.HasValue)
        {
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(result.Pd7Month.Value);
            message += $" No saved PD7 record existed for {monthName} {result.Pd7Year.Value}.";
        }

        return message;
    }

    public async Task<object?> BuildPaystubPreviewAsync()
    {
        if (SelectedEmployer is null || SelectedPayrollRecord is null)
        {
            Message = "Select a company and employee before previewing a paystub.";
            return null;
        }

        if (_requiresRecalculate)
        {
            Message = "Recalculate payroll deductions before previewing or printing.";
            return null;
        }

        var period = SelectedPayrollRecord.Period > 0
            ? SelectedPayrollRecord.Period
            : PayPeriodRules.NormalizePeriod(
                SelectedPayrollRecord.EmployerGroup,
                SelectedPayrollRecord.EmployerPaystubPeriod);
        var payPeriodRange = await _payPeriodRangeService.GetPayPeriodRangeAsync(
            SelectedPayrollRecord.EmployerGroup,
            period);
        var previewRecord = BuildPreviewRecordSnapshot(SelectedPayrollRecord);
        var vacationUsed = VacationSettings.IsPaidOut(VacationSetting) ? Vacation : 0m;

        return new PaystubTemplateBindingModel(
            SelectedEmployer,
            previewRecord,
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
            vacationUsed,
            VacationSetting,
            period,
            payPeriodRange.Beginning,
            payPeriodRange.Ending,
            DateTime.Today);
    }

    private PayrollRecordLookupDto BuildPreviewRecordSnapshot(PayrollRecordLookupDto source)
    {
        var taxYtdBeforeCurrent = Math.Max(source.TaxYtd - source.Tax1, 0m);
        var otherDeductionYtdBeforeCurrent = Math.Max(
            source.OtherDeductionYtd - source.OtherDeduction,
            0m);
        var vacationYtdBeforeCurrent = Math.Max(source.VacationYtd - source.Vacation, 0m);
        var netYtdBeforeCurrent = Math.Max(source.NetYtd - source.NetPay, 0m);

        return new PayrollRecordLookupDto
        {
            Id = source.Id,
            EmployeeNumber = source.EmployeeNumber,
            RpNumber = source.RpNumber,
            EmployeeName = source.EmployeeName,
            EmployerName = source.EmployerName,
            EmployerGroup = source.EmployerGroup,
            EmployerPaystubPeriod = source.EmployerPaystubPeriod,
            Period = source.Period,
            EmployeeAddress = source.EmployeeAddress,
            EmployeeCity = source.EmployeeCity,
            EmployeeProvince = source.EmployeeProvince,
            EmployeePostalCode = source.EmployeePostalCode,
            EmployeeEmailAddress = source.EmployeeEmailAddress,
            Salary = source.Salary,
            Rate = source.Rate,
            RegularHours = source.RegularHours,
            OvertimeHours = source.OvertimeHours,
            PublicHolidayHours = source.PublicHolidayHours,
            StatHolidayHours = source.StatHolidayHours,
            Vacation = source.Vacation,
            VacationSetting = source.VacationSetting,
            Gross = source.Gross,
            Tax1 = source.Tax1,
            Cpp1 = source.Cpp1,
            Cpp2 = source.Cpp2,
            Ei = source.Ei,
            OtherDeduction = source.OtherDeduction,
            NetPay = source.NetPay,
            GrossYtd = _grossYtdBeforeCurrent,
            NetYtd = netYtdBeforeCurrent,
            TaxYtd = taxYtdBeforeCurrent,
            Cpp1Ytd = _cpp1YtdBeforeCurrent,
            Cpp2Ytd = _cpp2YtdBeforeCurrent,
            EiYtd = _eiYtdBeforeCurrent,
            OtherDeductionYtd = otherDeductionYtdBeforeCurrent,
            VacationYtd = vacationYtdBeforeCurrent,
            VacationYtdUsed = _vacationYtdUsedBeforeCurrent,
            VacationYtdBalance = vacationYtdBeforeCurrent - _vacationYtdUsedBeforeCurrent,
            TotalPd7 = source.TotalPd7,
            HireDate = source.HireDate,
            DateOfBirth = source.DateOfBirth,
            EiExempt = source.EiExempt
        };
    }

    private void RefreshNetPay()
    {
        NetPay = CalculateNetPay(Gross, Vacation, VacationSetting, Tax1, Cpp1, Cpp2, Ei, OtherDeduction);
    }

    private void OpenCraWebsite()
    {
        Launcher.OpenAsync("https://www.canada.ca/en/revenue-agency.html");
    }

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

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
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
        RefreshNetPay();
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

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
