using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.EmployerUpdate;
using HWAY.Payroll.Services.EmployerInsert;
using Microsoft.Maui.Controls;
using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployerSearchViewModel : INotifyPropertyChanged
{
    private readonly IEmployerUpdateService _employerUpdateService;
    private bool _suppressSearchTextChanged;

    public EmployerSearchViewModel(IEmployerUpdateService employerUpdateService)
    {
        _employerUpdateService = employerUpdateService;

        Employers = new ObservableCollection<EmployerLookupDto>();

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async (emp) => await SelectEmployerAsync(emp));
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

    // 🔹 List
    public ObservableCollection<EmployerLookupDto> Employers { get; }

    // 🔹 Commands
    public ICommand SearchCommand { get; }
    public ICommand SelectEmployerCommand { get; }

    public async Task InitializeAsync()
    {
        _suppressSearchTextChanged = true;
        SearchText = "";
        _suppressSearchTextChanged = false;

        await SearchEmployersAsync();
    }

    private async Task SearchEmployersAsync()
    {
        var results = await _employerUpdateService.SearchEmployersAsync(SearchText);

        Employers.Clear();

        foreach (var emp in results)
            Employers.Add(emp);
    }

    private async Task SelectEmployerAsync(EmployerLookupDto employer)
    {
        if (employer == null)
            return;

        // Navigate to update page WITH selected employer
        await Shell.Current.GoToAsync(nameof(EmployerUpdatePage),
            new Dictionary<string, object>
            {
                ["Employer"] = employer
            });
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
