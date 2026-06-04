using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployeeInsert;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployeeInsertViewModel : INotifyPropertyChanged
{
    private readonly IEmployeeInsertService _employeeInsertService;

    public EmployeeInsertViewModel(IEmployeeInsertService employeeInsertService)
    {
        _employeeInsertService = employeeInsertService;

        SaveCommand = new Command(async () => await SaveEmployeeAsync());
    }

    // 🔹 Form fields (bind these to UI inputs)

    public string RpNumber { get; set; } = "";
    public string EmployeeNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public bool? IsActive { get; set; } = true;
    public string? Sin { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? EmailAddress { get; set; }

    // 🔹 UI state
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

    // 🔹 Command
    public ICommand SaveCommand { get; }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(RpNumber))
            return Fail("RP Number is required");

        if (string.IsNullOrWhiteSpace(EmployeeNumber))
            return Fail("Employee Number is required");

        if (string.IsNullOrWhiteSpace(FirstName))
            return Fail("First Name is required");

        if (string.IsNullOrWhiteSpace(LastName))
            return Fail("Last Name is required");

        if (!string.IsNullOrWhiteSpace(EmailAddress) && !EmailAddress.Contains("@"))
            return Fail("Email is invalid");

        return true;
    }

    private bool _isSuccess;
    public bool IsSuccess
    {
        get => _isSuccess;
        set => SetProperty(ref _isSuccess, value);
    }

    private bool Fail(string message)
    {
        Message = message;
        return false;
    }

    private async Task SaveEmployeeAsync()
    {
        if (IsBusy) return;

        if (!Validate())
            return;

        try
        {
            IsBusy = true;
            Message = "";

            var dto = new EmployeeInsertDto
            {
                RpNumber = RpNumber,
                EmployeeNumber = EmployeeNumber,
                FirstName = FirstName,
                LastName = LastName,
                IsActive = IsActive ?? true,
                Sin = Sin,
                Address = Address,
                City = City,
                Province = Province,
                PostalCode = PostalCode,
                EmailAddress = EmailAddress
            };

            var id = await _employeeInsertService.CreateEmployeeAsync(dto);

            IsSuccess = true;
            Message = $"Employee created successfully (ID: {id})";

            ClearForm();
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

    private void ClearForm()
    {
        RpNumber = "";
        EmployeeNumber = "";
        FirstName = "";
        LastName = "";
            IsActive = IsActive ?? true;

        OnPropertyChanged(nameof(RpNumber));
        OnPropertyChanged(nameof(EmployeeNumber));
        OnPropertyChanged(nameof(FirstName));
        OnPropertyChanged(nameof(LastName));
        OnPropertyChanged(nameof(Sin));
        OnPropertyChanged(nameof(Address));
        OnPropertyChanged(nameof(City));
        OnPropertyChanged(nameof(Province));
        OnPropertyChanged(nameof(PostalCode));
        OnPropertyChanged(nameof(EmailAddress));
    }

    // 🔹 Property change boilerplate
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