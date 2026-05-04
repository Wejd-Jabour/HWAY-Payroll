using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.Employers;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployerLookupViewModel : INotifyPropertyChanged
{
    private readonly IEmployerLookupService _employerLookupService;

    private string _searchText = string.Empty;
    private EmployerLookupDto? _selectedEmployer;
    private bool _isDropdownVisible;
    private bool _isBusy;
    private bool _isPaystubVisible;
    private bool _canShowPaystub;

    public EmployerLookupViewModel(IEmployerLookupService employerLookupService)
    {
        _employerLookupService = employerLookupService;

        Employers = new ObservableCollection<EmployerLookupDto>();

        SearchCommand = new Command(async () => await SearchEmployersAsync());
        SelectEmployerCommand = new Command<EmployerLookupDto>(async employer => await SelectEmployerAsync(employer));
        ShowPaystubCommand = new Command(ShowPaystub);

        _ = LoadInitialEmployersAsync();
    }

    public ObservableCollection<EmployerLookupDto> Employers { get; }

    public ICommand SearchCommand { get; }
    public ICommand SelectEmployerCommand { get; }
    public ICommand ShowPaystubCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                _ = SearchEmployersAsync();
            }
        }
    }

    public EmployerLookupDto? SelectedEmployer
    {
        get => _selectedEmployer;
        set => SetProperty(ref _selectedEmployer, value);
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

    public bool IsPaystubVisible
    {
        get => _isPaystubVisible;
        set => SetProperty(ref _isPaystubVisible, value);
    }

    public bool CanShowPaystub
    {
        get => _canShowPaystub;
        set => SetProperty(ref _canShowPaystub, value);
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
            {
                Employers.Add(employer);
            }

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

        SelectedEmployer = await _employerLookupService.GetEmployerByRpNumberAsync(employer.RpNumber);

        SearchText = employer.RpNumber;

        IsDropdownVisible = false;

        CanShowPaystub = SelectedEmployer is not null;

        // Hide the paystub when a new employer is selected.
        // The user can click the button again to preview it.
        IsPaystubVisible = false;
    }

    private void ShowPaystub()
    {
        if (SelectedEmployer is null)
            return;

        IsPaystubVisible = true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}