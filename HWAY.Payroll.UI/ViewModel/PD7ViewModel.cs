using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.PD7;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModel;

public class PD7ViewModel : INotifyPropertyChanged
{
    private readonly IPD7LookupService _pd7LookupService;

    public PD7ViewModel(IPD7LookupService pd7LookupService)
    {
        _pd7LookupService = pd7LookupService;

        Records = new ObservableCollection<PD7LookupDto>();

        SearchCommand = new Command(async () => await SearchAsync());
        InitializeCommand = new Command(async () => await InitializeAsync());
    }

    public ICommand SearchCommand { get; }
    public ICommand InitializeCommand { get; }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public ObservableCollection<PD7LookupDto> Records { get; }

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

    public async Task InitializeAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Message = "";

            var results = await _pd7LookupService.SearchPd7Async("");
            Records.Clear();
            foreach (var r in results)
                Records.Add(r);
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            Message = "";

            var results = await _pd7LookupService.SearchPd7Async(SearchText);
            Records.Clear();
            foreach (var r in results)
                Records.Add(r);

            if (results.Count == 0)
                Message = "No records found";
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}
