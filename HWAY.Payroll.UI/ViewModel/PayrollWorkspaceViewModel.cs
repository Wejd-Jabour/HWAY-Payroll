using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using HWAY.Payroll.Services.Employers;
using HWAY.Payroll.Services.PayrollRecord;

namespace HWAY.Payroll.UI.ViewModels;

public class PayrollWorkspaceViewModel : INotifyPropertyChanged
{
    private readonly IEmployerLookupService _employerLookupService;
    private readonly IPayrollRecordLookupService _payrollRecordService;

    private int _currentPayrollRecordId;
    private string _searchText = string.Empty;
    private EmployerLookupDto? _selectedEmployer;
    private PayrollRecordLookupDto? _selectedPayrollRecord;
    private bool _isDropdownVisible;
    private bool _isBusy;
    private bool _searchResultsFound;
    private bool _employeeFound;
    private bool _employeeNotFound;
    private bool _isPaystubVisible;
    private object? _previewPaystub;
    private bool _suppressSearchTextChanged;

    public PayrollWorkspaceViewModel(
        IEmployerLookupService employerLookupService,
        IPayrollRecordLookupService payrollRecordService)
    {
        _employerLookupService = employerLookupService;
        _payrollRecordService = payrollRecordService;

        Employers = new ObservableCollection<EmployerLookupDto>();
        SearchResults = new ObservableCollection<PayrollRecordLookupDto>();

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async employer => await SelectEmployerAsync(employer));
        CalculateAndOpenCraCommand = new Command(CalculateAndOpenCra);
        SaveCommand = new Command(async () => await SaveChanges());
        PreviewPaystubCommand = new Command(ShowPreviewPaystub);
        OpenCraWebsiteCommand = new Command(OpenCraWebsite);

        _ = LoadInitialEmployersAsync();
    }

    public ObservableCollection<EmployerLookupDto> Employers { get; }
    public ObservableCollection<PayrollRecordLookupDto> SearchResults { get; }

    public ICommand SearchCommand { get; }
    public ICommand SelectEmployerCommand { get; }
    public ICommand CalculateAndOpenCraCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand PreviewPaystubCommand { get; }
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
        set => SetProperty(ref _selectedEmployer, value);
    }

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

    public bool EmployeeFound
    {
        get => _employeeFound;
        set => SetProperty(ref _employeeFound, value);
    }

    public bool EmployeeNotFound
    {
        get => _employeeNotFound;
        set => SetProperty(ref _employeeNotFound, value);
    }

    public bool IsPaystubVisible
    {
        get => _isPaystubVisible;
        set => SetProperty(ref _isPaystubVisible, value);
    }

    public object? PreviewPaystub
    {
        get => _previewPaystub;
        set => SetProperty(ref _previewPaystub, value);
    }

    private string _employeeID = "";
    public string EmployeeID
    {
        get => _employeeID;
        set => SetProperty(ref _employeeID, value);
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
        set => SetProperty(ref _vacation, value);
    }

    private decimal _salary;
    public decimal Salary
    {
        get => _salary;
        set => SetProperty(ref _salary, value);
    }

    private decimal _rate;
    public decimal Rate
    {
        get => _rate;
        set => SetProperty(ref _rate, value);
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
        set => SetProperty(ref _cpp1, value);
    }

    private decimal _cpp2;
    public decimal Cpp2
    {
        get => _cpp2;
        set => SetProperty(ref _cpp2, value);
    }

    private decimal _ei;
    public decimal Ei
    {
        get => _ei;
        set => SetProperty(ref _ei, value);
    }

    private decimal _otherDeduction;
    public decimal OtherDeduction
    {
        get => _otherDeduction;
        set => SetProperty(ref _otherDeduction, value);
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

            var results = await _employerLookupService.SearchEmployersAsync(SearchText);

            Employers.Clear();

            foreach (var employer in results)
                Employers.Add(employer);

            IsDropdownVisible = Employers.Count > 0;
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

            _suppressSearchTextChanged = true;
            SearchText = employer.RpNumber;
            _suppressSearchTextChanged = false;

            await LoadEmployeesForSelectedEmployerAsync();

            EmployeeFound = false;
            IsPaystubVisible = false;
            PreviewPaystub = null;
        }
        finally
        {
            _suppressSearchTextChanged = false;
            IsBusy = false;
        }
    }

    private async Task LoadEmployeesForSelectedEmployerAsync()
    {
        SearchResults.Clear();

        if (SelectedEmployer is null)
            return;

        var results = await _payrollRecordService.SearchPayrollRecordsByRpNumberAsync(SelectedEmployer.RpNumber);

        foreach (var item in results)
            SearchResults.Add(item);

        SearchResultsFound = SearchResults.Count > 0;
        EmployeeNotFound = !SearchResultsFound;
    }

    private void LoadSelectedPayrollRecord(PayrollRecordLookupDto? selectedRecord)
    {
        if (selectedRecord == null)
            return;

        _currentPayrollRecordId = selectedRecord.Id;
        EmployeeID = selectedRecord.EmployeeNumber;

        Salary = selectedRecord.Salary;
        Rate = selectedRecord.Rate;
        RegularHours = selectedRecord.RegularHours;
        OvertimeHours = selectedRecord.OvertimeHours;
        PublicHolidayHours = selectedRecord.PublicHolidayHours;
        StatHolidayHours = selectedRecord.StatHolidayHours;
        Gross = selectedRecord.Gross;
        Vacation = selectedRecord.Vacation;

        Tax1 = selectedRecord.Tax1;
        Cpp1 = selectedRecord.Cpp1;
        Cpp2 = selectedRecord.Cpp2;
        Ei = selectedRecord.Ei;
        OtherDeduction = selectedRecord.OtherDeduction;

        EmployeeFound = true;
        EmployeeNotFound = false;
        IsPaystubVisible = false;
        PreviewPaystub = null;
    }

    private async Task SaveChanges()
    {
        if (_currentPayrollRecordId <= 0 || SelectedPayrollRecord is null)
            return;

        var selectedPayrollRecordId = _currentPayrollRecordId;

        var dto = new PayrollRecordUpdateDto
        {
            Salary = Salary,
            Rate = Rate,
            RegularHours = RegularHours,
            OvertimeHours = OvertimeHours,
            PublicHolidayHours = PublicHolidayHours,
            StatHolidayHours = StatHolidayHours,
            Gross = Gross,
            Vacation = Vacation,
            Tax1 = Tax1,
            Cpp1 = Cpp1,
            Cpp2 = Cpp2,
            Ei = Ei,
            OtherDeduction = OtherDeduction
        };

        await _payrollRecordService.UpdatePayrollRecordAsync(_currentPayrollRecordId, dto);

        await RefreshEmployeesAndKeepSelectionAsync(selectedPayrollRecordId);

        ShowPreviewPaystub();
    }

    private async Task RefreshEmployeesAndKeepSelectionAsync(int payrollRecordId)
    {
        if (SelectedEmployer is null)
            return;

        SearchResults.Clear();

        var results = await _payrollRecordService.SearchPayrollRecordsByRpNumberAsync(SelectedEmployer.RpNumber);

        PayrollRecordLookupDto? refreshedSelectedRecord = null;

        foreach (var item in results)
        {
            SearchResults.Add(item);

            if (item.Id == payrollRecordId)
            {
                refreshedSelectedRecord = item;
            }
        }

        SearchResultsFound = SearchResults.Count > 0;
        EmployeeNotFound = !SearchResultsFound;

        if (refreshedSelectedRecord is not null)
        {
            SelectedPayrollRecord = refreshedSelectedRecord;
            LoadSelectedPayrollRecord(refreshedSelectedRecord);
        }
    }

    private void CalculateAndOpenCra()
    {
        Gross = CalculateGross(Salary, Rate, RegularHours, OvertimeHours, PublicHolidayHours, StatHolidayHours);
        Vacation = Gross * 0.04m;

        OpenCraWebsite();
    }

    private void OpenCraWebsite()
    {
        Launcher.OpenAsync("https://apps.cra-arc.gc.ca/ebci/rhpd/beta/entry");
    }

    private void ShowPreviewPaystub()
    {
        if (SelectedEmployer is null || SelectedPayrollRecord is null)
            return;

        PreviewPaystub = new PaystubTemplateBindingModel(
            SelectedEmployer,
            SelectedPayrollRecord,
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
            OtherDeduction);

        IsPaystubVisible = true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
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
        decimal otherDeduction)
    {
        EmployerName = employer.EmployerName;
        Address = employer.Address ?? "";
        City = employer.City ?? "";
        PostalCode = employer.PostalCode ?? "";
        EmployerGroup = employer.EmployerGroup ?? "";

        EmployeeNumber = record.EmployeeNumber;
        EmployeeName = record.EmployeeName;
        EmployeeAddressLine = "";

        PayDate = DateTime.Today.ToString("yyyy-MM-dd");
        PayPeriod = "";
        PeriodBeginning = "";
        PeriodEnding = "";

        var regularPay = salary + rate * regularHours;
        var overtimePay = rate * 1.5m * overtimeHours;
        var publicHolidayPay = rate * publicHolidayHours;
        var statHolidayPay = rate * 1.5m * statHolidayHours;

        RegularPaySalary = FormatMoney(salary);
        RegularPayHours = FormatNumber(regularHours);
        RegularPayRate = FormatMoney(rate);
        RegularPayCurrent = FormatMoney(regularPay);
        RegularPayYtd = FormatMoney(record.GrossYtd);

        OvertimePaySalary = "";
        OvertimePayHours = FormatNumber(overtimeHours);
        OvertimePayRate = FormatMoney(rate * 1.5m);
        OvertimePayCurrent = FormatMoney(overtimePay);
        OvertimePayYtd = "";

        PublicHolidaySalary = "";
        PublicHolidayHours = FormatNumber(publicHolidayHours);
        PublicHolidayRate = FormatMoney(rate);
        PublicHolidayCurrent = FormatMoney(publicHolidayPay);
        PublicHolidayYtd = "";

        StatHolidaySalary = "";
        StatHolidayHours = FormatNumber(statHolidayHours);
        StatHolidayRate = FormatMoney(rate * 1.5m);
        StatHolidayCurrent = FormatMoney(statHolidayPay);
        StatHolidayYtd = "";

        VacationPayCurrent = FormatMoney(vacation);
        VacationPayYtd = FormatMoney(record.VacationYtd);

        TotalPayCurrent = FormatMoney(gross + vacation);
        TotalPayYtd = FormatMoney(record.GrossYtd + record.VacationYtd);

        TaxCurrent = FormatMoney(tax1);
        TaxYtd = FormatMoney(record.TaxYtd);

        Cpp1Current = FormatMoney(cpp1);
        Cpp1Ytd = FormatMoney(record.Cpp1Ytd);

        Cpp2Current = FormatMoney(cpp2);
        Cpp2Ytd = FormatMoney(record.Cpp2Ytd);

        EiCurrent = FormatMoney(ei);
        EiYtd = FormatMoney(record.EiYtd);

        OtherDeductionCurrent = FormatMoney(otherDeduction);
        OtherDeductionYtd = FormatMoney(record.OtherDeductionYtd);

        var payrollDeductions = tax1 + cpp1 + cpp2 + ei;
        var totalDeductions = payrollDeductions + otherDeduction;
        var totalPay = gross + vacation;
        var netPay = CalculateNetPay(gross, vacation, tax1, cpp1, cpp2, ei, otherDeduction);

        HeaderNetPay = FormatMoney(netPay);
        BenefitNetPay = FormatMoney(netPay);

        SummaryTotalPayCurrent = FormatMoney(totalPay);
        SummaryTotalPayYtd = FormatMoney(record.GrossYtd + record.VacationYtd);

        SummaryTaxesCurrent = FormatMoney(tax1);
        SummaryTaxesYtd = FormatMoney(record.TaxYtd);

        SummaryDeductionsCurrent = FormatMoney(totalDeductions);
        SummaryDeductionsYtd = FormatMoney(
            record.TaxYtd +
            record.Cpp1Ytd +
            record.Cpp2Ytd +
            record.EiYtd +
            record.OtherDeductionYtd);

        SummaryNetPayCurrent = FormatMoney(netPay);
        SummaryNetPayYtd = FormatMoney(record.NetYtd);

        VacationYtdBalance = FormatMoney(record.VacationYtdBalance);
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

    public string PayDate { get; }
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
        decimal tax1,
        decimal cpp1,
        decimal cpp2,
        decimal ei,
        decimal otherDeduction)
    {
        var totalDeductions = tax1 + cpp1 + cpp2 + ei + otherDeduction;

        return gross + vacation - totalDeductions;
    }

    private static string FormatMoney(decimal value) => value == 0 ? "" : value.ToString("0.00");
    private static string FormatNumber(decimal value) => value == 0 ? "" : value.ToString("0.##");
}
