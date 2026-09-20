using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployerUpdate;
using HWAY.Payroll.Services.EmployerInsert;
using HWAY.Payroll.Services.EmployerRemove;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployerRemoveViewModel : INotifyPropertyChanged
{
    private readonly IEmployerRemoveService _employerRemoveService;
    private readonly IEmployerUpdateService _employerUpdateService;
    private bool _suppressSearchTextChanged;

        public EmployerRemoveViewModel(
        IEmployerRemoveService employerRemoveService,
        IEmployerUpdateService employerUpdateService)
    {
        _employerRemoveService = employerRemoveService;
        _employerUpdateService = employerUpdateService;

        Employers = new ObservableCollection<EmployerLookupDto>();

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        DeleteCommand = new Command<EmployerLookupDto>(async (emp) => await DeleteEmployerAsync(emp));
    }

    // 🔹 Search input
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

    // 🔹 List of employers
    public ObservableCollection<EmployerLookupDto> Employers { get; }

    // 🔹 UI state
    private string _message = "";
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    // 🔹 Commands
    public ICommand SearchCommand { get; }
    public ICommand DeleteCommand { get; }

    public async Task InitializeAsync()
    {
        if (IsBusy)
            return;

        _suppressSearchTextChanged = true;
        SearchText = "";
        _suppressSearchTextChanged = false;
        Message = "";

        await SearchEmployersAsync();
    }

    // 🔍 Search employees
    private async Task SearchEmployersAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await LoadEmployersAsync();
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

    private async Task LoadEmployersAsync()
    {
        var results = await _employerUpdateService.SearchEmployersAsync(SearchText);

        Employers.Clear();

        foreach (var emp in results)
            Employers.Add(emp);
    }

    // 🗑 Delete employer
    private async Task DeleteEmployerAsync(EmployerLookupDto employer)
    {
        if (employer == null || IsBusy)
            return;

        if (Application.Current?.MainPage is null)
        {
            Message = "Unable to show delete confirmation.";
            return;
        }

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Confirm Delete",
            $"Delete {employer.EmployerName}?",
            "Yes",
            "No");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;

            var dto = new EmployerRemoveDto
            {
                RpNumber = employer.RpNumber,
                EmployerName = employer.EmployerName
            };

            await _employerRemoveService.RemoveEmployerAsync(dto);

            await LoadEmployersAsync();

            Message = "Employer deleted successfully.";
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

    // 🔹 Property changed boilerplate
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (Equals(field, value)) return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}
