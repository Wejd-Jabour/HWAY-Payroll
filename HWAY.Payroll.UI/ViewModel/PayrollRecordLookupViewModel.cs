using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.PayrollRecord;

namespace HWAY.Payroll.UI.ViewModels;

public class PayrollRecordViewModel : INotifyPropertyChanged
{
    private readonly IPayrollRecordLookupService _service;

    public PayrollRecordViewModel(IPayrollRecordLookupService service)
    {
        _service = service;

        SearchEmployeeCommand = new Command(async () => await SearchEmployee());
        OpenCraWebsiteCommand = new Command(OpenCraWebsite);
    }

    // -------------------------
    // INPUT FIELD
    // -------------------------
    private string _employeeID = "";
    public string EmployeeID
    {
        get => _employeeID;
        set => SetProperty(ref _employeeID, value);
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
    public ICommand SearchEmployeeCommand { get; }
    public ICommand OpenCraWebsiteCommand { get; }

    // -------------------------
    // SEARCH LOGIC
    // -------------------------
    private async Task SearchEmployee()
    {
        if (string.IsNullOrWhiteSpace(EmployeeID))
            return;

        var result = await _service.GetPayrollRecordByEmployeeNumberAsync(EmployeeID);

        if (result == null)
        {
            EmployeeFound = false;
            EmployeeNotFound = true;
            return;
        }

        EmployeeFound = true;
        EmployeeNotFound = false;

        // map DTO → ViewModel properties
        Gross = result.Gross;
        Vacation = result.Vacation;
        Salary = result.Salary;
        Rate = result.Rate;
        RegularHours = result.RegularHours;
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
    public event PropertyChangedEventHandler PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}