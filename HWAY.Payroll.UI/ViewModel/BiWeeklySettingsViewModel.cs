using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HWAY.Payroll.Services.PayPeriods;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModel;

public sealed class BiWeeklySettingsViewModel : INotifyPropertyChanged
{
    private readonly IBiWeeklyScheduleService _scheduleService;
    private bool _isBusy;
    private string _message = "";

    public BiWeeklySettingsViewModel(IBiWeeklyScheduleService scheduleService)
    {
        _scheduleService = scheduleService;

        ScheduleA = new BiWeeklyScheduleEditor(
            "Bi-weekly A",
            BiWeeklyTable.A,
            DateTime.Today.AddDays(-14),
            _scheduleService);

        ScheduleB = new BiWeeklyScheduleEditor(
            "Bi-weekly B",
            BiWeeklyTable.B,
            DateTime.Today.AddDays(-14),
            _scheduleService);

        SaveACommand = new Command(async () => await SaveAsync(ScheduleA));
        SaveBCommand = new Command(async () => await SaveAsync(ScheduleB));
        SaveBothCommand = new Command(async () => await SaveBothAsync());
        ReloadCommand = new Command(async () => await LoadSavedSchedulesAsync());

        _ = LoadSavedSchedulesAsync();
    }

    public BiWeeklyScheduleEditor ScheduleA { get; }
    public BiWeeklyScheduleEditor ScheduleB { get; }

    public ICommand SaveACommand { get; }
    public ICommand SaveBCommand { get; }
    public ICommand SaveBothCommand { get; }
    public ICommand ReloadCommand { get; }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private async Task LoadSavedSchedulesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Message = "";

            var savedA = await _scheduleService.GetSavedScheduleAsync(BiWeeklyTable.A);
            var savedB = await _scheduleService.GetSavedScheduleAsync(BiWeeklyTable.B);

            if (savedA is not null)
            {
                ScheduleA.ApplySchedule(savedA);
            }

            if (savedB is not null)
            {
                ScheduleB.ApplySchedule(savedB);
            }

            Message = savedA is null && savedB is null
                ? "No saved bi-weekly schedules found yet."
                : "Loaded saved bi-weekly schedules.";
        }
        catch (Exception ex)
        {
            Message = $"Unable to load saved schedules: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveBothAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Message = "";

            await _scheduleService.SaveScheduleAsync(ScheduleA.Table, ScheduleA.LastPeriodEnding);
            await _scheduleService.SaveScheduleAsync(ScheduleB.Table, ScheduleB.LastPeriodEnding);

            Message = "Saved both bi-weekly schedules.";
        }
        catch (Exception ex)
        {
            Message = $"Unable to save schedules: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync(BiWeeklyScheduleEditor schedule)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Message = "";

            await _scheduleService.SaveScheduleAsync(schedule.Table, schedule.LastPeriodEnding);
            Message = $"Saved {schedule.Name}.";
        }
        catch (Exception ex)
        {
            Message = $"Unable to save {schedule.Name}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}

public sealed class BiWeeklyScheduleEditor : INotifyPropertyChanged
{
    private readonly IBiWeeklyScheduleService _scheduleService;
    private DateTime _lastPeriodEnding;
    private DateTime _firstPeriodBeginning;
    private string _threePeriodMonths = "";
    private string _scheduleRange = "";

    public BiWeeklyScheduleEditor(
        string name,
        BiWeeklyTable table,
        DateTime lastPeriodEnding,
        IBiWeeklyScheduleService scheduleService)
    {
        Name = name;
        Table = table;
        _scheduleService = scheduleService;
        Periods = new ObservableCollection<BiWeeklyPeriodPreviewRow>();

        _lastPeriodEnding = lastPeriodEnding.Date;
        RefreshPreview();
    }

    public string Name { get; }
    public BiWeeklyTable Table { get; }
    public ObservableCollection<BiWeeklyPeriodPreviewRow> Periods { get; }

    public DateTime LastPeriodEnding
    {
        get => _lastPeriodEnding;
        set
        {
            if (SetProperty(ref _lastPeriodEnding, value.Date))
            {
                RefreshPreview();
            }
        }
    }

    public DateTime FirstPeriodBeginning
    {
        get => _firstPeriodBeginning;
        private set => SetProperty(ref _firstPeriodBeginning, value);
    }

    public string FirstPeriodBeginningDisplay => FirstPeriodBeginning.ToString("yyyy-MM-dd");

    public string ThreePeriodMonths
    {
        get => _threePeriodMonths;
        private set => SetProperty(ref _threePeriodMonths, value);
    }

    public string ScheduleRange
    {
        get => _scheduleRange;
        private set => SetProperty(ref _scheduleRange, value);
    }

    public void ApplySchedule(BiWeeklySchedulePreview schedule)
    {
        _lastPeriodEnding = schedule.LastPeriodEnding;
        OnPropertyChanged(nameof(LastPeriodEnding));
        ApplyPreview(schedule);
    }

    private void RefreshPreview()
    {
        ApplyPreview(_scheduleService.GenerateSchedule(LastPeriodEnding));
    }

    private void ApplyPreview(BiWeeklySchedulePreview schedule)
    {
        FirstPeriodBeginning = schedule.FirstPeriodBeginning;
        OnPropertyChanged(nameof(FirstPeriodBeginningDisplay));

        ThreePeriodMonths = schedule.ThreePeriodMonths.Count == 0
            ? "None"
            : string.Join(", ", schedule.ThreePeriodMonths.Select(FormatMonth));

        var firstEnding = schedule.Periods.First().Ending.ToString("yyyy-MM-dd");
        var lastEnding = schedule.Periods.Last().Ending.ToString("yyyy-MM-dd");
        ScheduleRange = $"{firstEnding} to {lastEnding}";

        Periods.Clear();

        foreach (var period in schedule.Periods)
        {
            Periods.Add(new BiWeeklyPeriodPreviewRow(
                period.Number,
                period.Beginning,
                period.Ending,
                schedule.ThreePeriodMonths.Contains(period.Ending.Month)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private static string FormatMonth(int month)
    {
        var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        return $"{month:00} - {monthName}";
    }
}

public sealed class BiWeeklyPeriodPreviewRow
{
    public BiWeeklyPeriodPreviewRow(int number, DateTime beginning, DateTime ending, bool isThreePeriodMonth)
    {
        Number = number;
        Beginning = beginning;
        Ending = ending;
        IsThreePeriodMonth = isThreePeriodMonth;
    }

    public int Number { get; }
    public DateTime Beginning { get; }
    public DateTime Ending { get; }
    public bool IsThreePeriodMonth { get; }

    public string PeriodDisplay => Number.ToString("00");
    public string BeginningDisplay => Beginning.ToString("yyyy-MM-dd");
    public string EndingDisplay => Ending.ToString("yyyy-MM-dd");
    public string ThreePeriodMonthDisplay => IsThreePeriodMonth ? "Yes" : "";
}
