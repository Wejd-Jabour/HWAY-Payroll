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

        SaveCommand = new Command(async () => await SaveAsync());
        ClearCommand = new Command(ClearForm);
    }

    public ICommand SaveCommand { get; }
    public ICommand ClearCommand { get; }

    private string _rpNumber = "";
    public string RpNumber
    {
        get => _rpNumber;
        set => SetProperty(ref _rpNumber, value);
    }

    private decimal _fee;
    public decimal Fee
    {
        get => _fee;
        set => SetProperty(ref _fee, value);
    }

    private decimal _gross;
    public decimal Gross
    {
        get => _gross;
        set => SetProperty(ref _gross, value);
    }

    private decimal _pd7Total;
    public decimal PD7Total
    {
        get => _pd7Total;
        set => SetProperty(ref _pd7Total, value);
    }

    private int _activeEmployees;
    public int ActiveEmployees
    {
        get => _activeEmployees;
        set => SetProperty(ref _activeEmployees, value);
    }

    private string _period = "";
    public string Period
    {
        get => _period;
        set => SetProperty(ref _period, value);
    }

    private decimal _transfer;
    public decimal Transfer
    {
        get => _transfer;
        set => SetProperty(ref _transfer, value);
    }

    private decimal _received;
    public decimal Received
    {
        get => _received;
        set => SetProperty(ref _received, value);
    }

    private bool _feeCheck;
    public bool FeeCheck
    {
        get => _feeCheck;
        set => SetProperty(ref _feeCheck, value);
    }

    private int _month;
    public int Month
    {
        get => _month;
        set => SetProperty(ref _month, value);
    }

    private int _year;
    public int Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }

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

    private async Task SaveAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Message = "";

            if (string.IsNullOrWhiteSpace(RpNumber))
            {
                Message = "RP Number is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Period))
            {
                Message = "Period is required.";
                return;
            }

            if (Fee < 0 || Gross < 0 || PD7Total < 0 || Transfer < 0 || Received < 0 || ActiveEmployees < 0 || Month < 0 || Year < 0)
            {
                Message = "Negative values are not allowed.";
                return;
            }

            var dto = new PD7LookupDto
            {
                RPNumber = RpNumber,
                Fee = Fee,
                Gross = Gross,
                PD7Total = PD7Total,
                ActiveEmployees = ActiveEmployees,
                Period = Period,
                Transfer = Transfer,
                Received = Received,
                FeeCheck = FeeCheck,
                Month = Month,
                Year = Year
            };

            var saved = await _pd7LookupService.CreatePd7Async(dto);
            Message = saved ? "PD7 saved successfully." : "Unable to save PD7 record.";
            if (saved) ClearForm();
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

    private void ClearForm()
    {
        RpNumber = "";
        Fee = 0;
        Gross = 0;
        PD7Total = 0;
        ActiveEmployees = 0;
        Period = "";
        Transfer = 0;
        Received = 0;
        FeeCheck = false;
        Month = 0;
        Year = 0;
        Message = "";
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
