using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using HWAY.Payroll.Services.PayrollRecord;
using HWAY.Payroll.Services.PayrollRecordEdit;

namespace HWAY.Payroll.UI.ViewModels;

public class PayrollRecordEditViewModel : INotifyPropertyChanged
{
    private readonly IPayrollRecordEditService _service;
    private readonly IPayrollRecordLookupService _lookupService;

    private int _currentPayrollRecordId;
    private bool _requiresRecalculate;

    public PayrollRecordEditViewModel(IPayrollRecordEditService service, IPayrollRecordLookupService lookupService)
    {
        _service = service;
        _lookupService = lookupService;

        SearchCommand = new Command(async () => await SearchPayrollRecords());
        SaveCommand = new Command(async () => await SaveChanges());
        CalculatePayrollCommand = new Command(CalculatePayroll);
        OpenCraWebsiteCommand = new Command(OpenCraWebsite);

        SearchResults = new ObservableCollection<PayrollRecordLookupDto>();
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
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
    public ICommand SaveCommand { get; }
    public ICommand CalculatePayrollCommand { get; }
    public ICommand OpenCraWebsiteCommand { get; }

    public ObservableCollection<PayrollRecordLookupDto> SearchResults { get; }

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

    private async Task SearchPayrollRecords()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            SearchResults.Clear();
            SearchResultsFound = false;
            EmployeeFound = false;
            EmployeeNotFound = false;
            Message = "Enter employee or employer search text.";
            return;
        }

        var results = await _lookupService.SearchPayrollRecordsAsync(SearchText.Trim());

        SearchResults.Clear();
        foreach (var result in results)
        {
            SearchResults.Add(result);
        }

        SearchResultsFound = SearchResults.Count > 0;
        EmployeeFound = false;
        EmployeeNotFound = !SearchResultsFound;
        Message = SearchResultsFound ? string.Empty : "No matching payroll records found.";
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

        Gross = selectedRecord.Gross;
        Vacation = selectedRecord.Vacation;
        _requiresRecalculate = false;

        Tax1 = selectedRecord.Tax1;
        Cpp1 = selectedRecord.Cpp1;
        Cpp2 = selectedRecord.Cpp2;
        Ei = selectedRecord.Ei;
        OtherDeduction = selectedRecord.OtherDeduction;

        NetPay = selectedRecord.NetPay;

        EmployeeFound = true;
        EmployeeNotFound = false;
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
            Message = "Recalculate gross and vacation before saving by pressing the CRA button.";
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
                Gross = Gross,
                Vacation = Vacation,
                Tax1 = Tax1,
                Cpp1 = Cpp1,
                Cpp2 = Cpp2,
                Ei = Ei,
                OtherDeduction = OtherDeduction,
                NetPay = NetPay
            };

            await _service.UpdatePayrollRecordAsync(_currentPayrollRecordId, dto);

            Message = "Payroll record updated successfully.";
            EmployeeFound = false;
            SearchResults.Clear();
            SearchResultsFound = false;
            _currentPayrollRecordId = 0;
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
    }

    private void CalculatePayroll()
    {
        Gross = Salary + (Rate * RegularHours) + (Rate * 1.5m * OvertimeHours);
        Vacation = Gross * 0.04m;
        NetPay = Gross - Tax1 - Cpp1 - Cpp2 - Ei - OtherDeduction;
        _requiresRecalculate = false;
        Message = "Gross and vacation recalculated. Opening CRA...";
        OpenCraWebsite();
    }

    private void OpenCraWebsite()
    {
        Launcher.OpenAsync("https://www.canada.ca/en/revenue-agency.html");
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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
