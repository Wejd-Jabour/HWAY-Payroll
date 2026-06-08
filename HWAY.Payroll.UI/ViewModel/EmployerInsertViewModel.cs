using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployerInsert;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployerInsertViewModel : INotifyPropertyChanged
{
    private readonly IEmployerInsertService _employerInsertService;

    public EmployerInsertViewModel(IEmployerInsertService employerInsertService)
    {
        _employerInsertService = employerInsertService;

        SaveCommand = new Command(async () => await SaveEmployerAsync());
    }

    // 🔹 Form fields (bind these to UI inputs)

    public string RpNumber { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }

    private string? _employerGroup = EmployerGroupOption.Default.Code;
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

    public decimal Fee { get; set; }
    

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

        if (string.IsNullOrWhiteSpace(EmployerName))
            return Fail("Employer Name is required");

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

    private async Task SaveEmployerAsync()
    {
        if (IsBusy) return;

        if (!Validate())
            return;

        try
        {
            IsBusy = true;
            Message = "";

            var dto = new EmployerInsertDto
            {
                RpNumber = RpNumber,
                EmployerName = EmployerName,
                Address = Address,
                City = City,
                PostalCode = PostalCode,
                EmployerGroup = EmployerGroup,
                Fee = Fee
            };

            var id = await _employerInsertService.CreateEmployerAsync(dto);

            IsSuccess = true;
            Message = $"Employer created successfully (ID: {id})";

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
        EmployerName = "";
        Address = null;
        City = null;
        PostalCode = null;
        EmployerGroup = EmployerGroupOption.Default.Code;
        Fee = 0;

        OnPropertyChanged(nameof(RpNumber));
        OnPropertyChanged(nameof(EmployerName));
        OnPropertyChanged(nameof(Address));
        OnPropertyChanged(nameof(City));
        OnPropertyChanged(nameof(PostalCode));
        OnPropertyChanged(nameof(EmployerGroup));
        OnPropertyChanged(nameof(Fee));
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
