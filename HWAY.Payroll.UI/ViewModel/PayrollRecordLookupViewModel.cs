using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using HWAY.Payroll.Services.PayrollRecord;

namespace HWAY.Payroll.UI.ViewModels;

public class PayrollRecordViewModel : INotifyPropertyChanged
{
    private readonly IPayrollRecordLookupService _service;
    private int _currentPayrollRecordId;
    private string _searchRpNumber = string.Empty;
    private PayrollRecordLookupDto? _selectedPayrollRecord;

    public PayrollRecordViewModel(IPayrollRecordLookupService service)
    {
        _service = service;

        SearchByRpNumberCommand = new Command(async () => await SearchByRpNumber());
        CalculateAndOpenCraCommand = new Command(CalculateAndOpenCra);
        SaveCommand = new Command(async () => await SaveChanges());
        OpenCraWebsiteCommand = new Command(OpenCraWebsite);

        SearchResults = new ObservableCollection<PayrollRecordLookupDto>();
    }

    // -------------------------
    // SEARCH FIELD
    // -------------------------
    private string _employeeID = "";
    public string EmployeeID
    {
        get => _employeeID;
        set => SetProperty(ref _employeeID, value);
    }

    private string _rpNumber = "";
    public string RpNumber
    {
        get => _rpNumber;
        set => SetProperty(ref _rpNumber, value);
    }

    // -------------------------
    // PAYROLL FIELDS (BOUND TO UI)
    // -------------------------
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

    // -------------------------
    // UI STATE FLAGS
    // -------------------------
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

    // -------------------------
    // COMMANDS
    // -------------------------
    public ICommand SearchByRpNumberCommand { get; }
    public ICommand CalculateAndOpenCraCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand OpenCraWebsiteCommand { get; }

    // -------------------------
    // SEARCH RESULTS
    // -------------------------
    public ObservableCollection<PayrollRecordLookupDto> SearchResults { get; }

    private bool _searchResultsFound;
    public bool SearchResultsFound
    {
        get => _searchResultsFound;
        set => SetProperty(ref _searchResultsFound, value);
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

    // -------------------------
    // SEARCH LOGIC
    // -------------------------
    private async Task SearchByRpNumber()
    {
        if (string.IsNullOrWhiteSpace(RpNumber))
            return;

        var results = await _service.SearchPayrollRecordsByRpNumberAsync(RpNumber);

        SearchResults.Clear();
        foreach (var item in results)
        {
            SearchResults.Add(item);
        }

        SearchResultsFound = SearchResults.Count > 0;
        EmployeeFound = false;
        EmployeeNotFound = false;

        if (!SearchResultsFound)
        {
            EmployeeNotFound = true;
        }
    }

    private void LoadSelectedPayrollRecord(PayrollRecordLookupDto? selectedRecord)
    {
        if (selectedRecord == null)
            return;

        _currentPayrollRecordId = selectedRecord.Id;
        EmployeeID = selectedRecord.EmployeeNumber;
        RpNumber = selectedRecord.RpNumber;

        Salary = selectedRecord.Salary;
        Rate = selectedRecord.Rate;
        RegularHours = selectedRecord.RegularHours;
        Gross = selectedRecord.Gross;
        Vacation = selectedRecord.Vacation;

        Tax1 = selectedRecord.Tax1;
        Cpp1 = selectedRecord.Cpp1;
        Cpp2 = selectedRecord.Cpp2;
        Ei = selectedRecord.Ei;
        OtherDeduction = selectedRecord.OtherDeduction;

        EmployeeFound = true;
        EmployeeNotFound = false;
    }

    // -------------------------
    // SAVE CHANGES
    // -------------------------
    private async Task SaveChanges()
    {
        if (_currentPayrollRecordId <= 0)
            return;

        var dto = new PayrollRecordUpdateDto
        {
            Salary = Salary,
            Rate = Rate,
            RegularHours = RegularHours,
            Gross = Gross,
            Vacation = Vacation,
            Tax1 = Tax1,
            Cpp1 = Cpp1,
            Cpp2 = Cpp2,
            Ei = Ei,
            OtherDeduction = OtherDeduction
        };

        await _service.UpdatePayrollRecordAsync(_currentPayrollRecordId, dto);

        // Optionally, show a success message or something
    }

    // -------------------------
    // CALCULATE AND OPEN CRA
    // -------------------------
    private void CalculateAndOpenCra()
    {
        // Calculate Gross: Salary + (Rate * RegularHours)
        Gross = Salary + (Rate * RegularHours);

        // Calculate Vacation: 4% of Gross (adjust as needed)
        Vacation = Gross * 0.04m;

        // Open CRA website
        OpenCraWebsite();
    }

    // -------------------------
    // CRA LINK
    // -------------------------
    private void OpenCraWebsite()
    {
        Launcher.OpenAsync("https://www.canada.ca/en/revenue-agency.html");
    }

    // -------------------------
    // PROPERTY CHANGED HELPER
    // -------------------------
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}