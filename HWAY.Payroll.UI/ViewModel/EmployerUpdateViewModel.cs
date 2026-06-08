using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployerUpdate;
using HWAY.Payroll.UI.Views;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModels;

[QueryProperty(nameof(Employer), "Employer")]
public class EmployerUpdateViewModel : INotifyPropertyChanged
{
    private readonly IEmployerUpdateService _employerUpdateService;

    public EmployerUpdateViewModel(IEmployerUpdateService employerUpdateService)
    {
        _employerUpdateService = employerUpdateService;

        SaveCommand = new Command(async () => await SaveEmployerAsync());
    }

    // 🔹 Incoming selected employer from navigation
    private EmployerLookupDto? _employer;
    public EmployerLookupDto? Employer
    {
        get => _employer;
        set
        {
            _employer = value;
            LoadEmployer(value);
        }
    }

    // 🔹 Editable fields
    private string _rpNumber = "";
    public string RpNumber
    {
        get => _rpNumber;
        set => SetProperty(ref _rpNumber, value);
    }

    private string _employerName = "";
    public string EmployerName
    {
        get => _employerName;
        set => SetProperty(ref _employerName, value);
    }

    private string? _address;
    public string? Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    private string? _city;
    public string? City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }

    private string? _postalCode;
    public string? PostalCode
    {
        get => _postalCode;
        set => SetProperty(ref _postalCode, value);
    }

    private string? _employerGroup;
    public string? EmployerGroup
    {
        get => _employerGroup;
        set
        {
            if (SetProperty(ref _employerGroup, EmployerGroupOption.NormalizeCode(value)))
            {
                OnPropertyChanged(nameof(SelectedEmployerGroupOption));
            }
        }
    }

    public IReadOnlyList<EmployerGroupOption> EmployerGroupOptions => EmployerGroupOption.All;

    public EmployerGroupOption SelectedEmployerGroupOption
    {
        get => EmployerGroupOption.FromCode(EmployerGroup);
        set => EmployerGroup = value?.Code;
    }

    private decimal _fee;
    public decimal Fee
    {
        get => _fee;
        set => SetProperty(ref _fee, value);
    }

    private int _paystubPeriod = 1;
    public int PaystubPeriod
    {
        get => _paystubPeriod;
        set => SetProperty(ref _paystubPeriod, value);
    }

    // 🔹 UI state
    private string _message = "";
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private bool _isSuccess;
    public bool IsSuccess
    {
        get => _isSuccess;
        set => SetProperty(ref _isSuccess, value);
    }

    public ICommand SaveCommand { get; }

    private async Task SaveEmployerAsync()
    {
        try
        {
            var dto = new EmployerUpdateDto
            {
                RpNumber = RpNumber,
                EmployerName = EmployerName,
                Address = Address,
                City = City,
                PostalCode = PostalCode,
                EmployerGroup = EmployerGroup,
                PaystubPeriod = PaystubPeriod,
                Fee = Fee
            };

            var id = await _employerUpdateService.UpdateEmployerAsync(dto);

            IsSuccess = true;
            Message = $"Employer updated successfully (ID: {id})";
        }
        catch (Exception ex)
        {
            IsSuccess = false;
            Message = ex.Message;
        }
    }

    private void LoadEmployer(EmployerLookupDto? employer)
    {
        if (employer == null)
            return;

        RpNumber = employer.RpNumber;
        EmployerName = employer.EmployerName;
        Address = employer.Address;
        City = employer.City;
        PostalCode = employer.PostalCode;
        EmployerGroup = employer.EmployerGroup;
        PaystubPeriod = employer.PaystubPeriod;
        Fee = employer.Fee;
    }

    // 🔹 Property changed boilerplate
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (Equals(field, value)) return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }

    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
