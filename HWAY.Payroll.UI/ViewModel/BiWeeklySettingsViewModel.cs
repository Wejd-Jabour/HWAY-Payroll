using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Input;
using HWAY.Payroll.Services.Calculations;
using HWAY.Payroll.Services.PayPeriods;
using Microsoft.Maui.Controls;

namespace HWAY.Payroll.UI.ViewModel;

public sealed class BiWeeklySettingsViewModel : INotifyPropertyChanged
{
    private readonly IBiWeeklyScheduleService _scheduleService;
    private readonly ICalculationSettingsService _calculationSettingsService;
    private bool _isBusy;
    private string _message = "";
    private CalculationEditorRow? _selectedCalculation;
    private JsonObject? _guidedCalculationJson;
    private JsonObject? _guidedPeriodJson;
    private DateTime _guidedEffectiveDate = DateTime.Today;
    private string _selectedProvinceCode = "";
    private string _newCalculationName = "";
    private string _newCalculation = "";

    public BiWeeklySettingsViewModel(
        IBiWeeklyScheduleService scheduleService,
        ICalculationSettingsService calculationSettingsService)
    {
        _scheduleService = scheduleService;
        _calculationSettingsService = calculationSettingsService;

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
        Calculations = new ObservableCollection<CalculationEditorRow>();
        GuidedFields = new ObservableCollection<CalculationSimpleField>();
        FederalFields = new ObservableCollection<CalculationSimpleField>();
        FederalTaxBrackets = new ObservableCollection<TaxBracketEditorRow>();
        ProvinceCodes = new ObservableCollection<string>();
        ProvinceFields = new ObservableCollection<CalculationSimpleField>();
        ProvinceTaxBrackets = new ObservableCollection<TaxBracketEditorRow>();
        HealthPremiumBands = new ObservableCollection<HealthPremiumEditorRow>();

        SaveACommand = new Command(async () => await SaveAsync(ScheduleA));
        SaveBCommand = new Command(async () => await SaveAsync(ScheduleB));
        SaveBothCommand = new Command(async () => await SaveBothAsync());
        ReloadCommand = new Command(async () => await LoadSavedSchedulesAsync());
        ReloadCalculationsCommand = new Command(async () => await LoadCalculationsAsync());
        SaveCalculationCommand = new Command(async () => await SaveSelectedCalculationAsync());
        AddCalculationCommand = new Command(async () => await AddCalculationAsync());

        _ = LoadInitialSettingsAsync();
    }

    public BiWeeklyScheduleEditor ScheduleA { get; }
    public BiWeeklyScheduleEditor ScheduleB { get; }
    public ObservableCollection<CalculationEditorRow> Calculations { get; }
    public ObservableCollection<CalculationSimpleField> GuidedFields { get; }
    public ObservableCollection<CalculationSimpleField> FederalFields { get; }
    public ObservableCollection<TaxBracketEditorRow> FederalTaxBrackets { get; }
    public ObservableCollection<string> ProvinceCodes { get; }
    public ObservableCollection<CalculationSimpleField> ProvinceFields { get; }
    public ObservableCollection<TaxBracketEditorRow> ProvinceTaxBrackets { get; }
    public ObservableCollection<HealthPremiumEditorRow> HealthPremiumBands { get; }

    public ICommand SaveACommand { get; }
    public ICommand SaveBCommand { get; }
    public ICommand SaveBothCommand { get; }
    public ICommand ReloadCommand { get; }
    public ICommand ReloadCalculationsCommand { get; }
    public ICommand SaveCalculationCommand { get; }
    public ICommand AddCalculationCommand { get; }

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

    public CalculationEditorRow? SelectedCalculation
    {
        get => _selectedCalculation;
        set
        {
            if (SetProperty(ref _selectedCalculation, value))
            {
                LoadGuidedCalculationEditor();
            }
        }
    }

    public DateTime GuidedEffectiveDate
    {
        get => _guidedEffectiveDate;
        set => SetProperty(ref _guidedEffectiveDate, value.Date);
    }

    public string SelectedProvinceCode
    {
        get => _selectedProvinceCode;
        set
        {
            if (SetProperty(ref _selectedProvinceCode, value))
            {
                LoadSelectedProvinceEditor();
                OnPropertyChanged(nameof(ShowHealthPremiumBands));
            }
        }
    }

    public bool IsKnownCalculationSelected =>
        IsCppCalculationSelected || IsEiCalculationSelected || IsTaxCalculationSelected;

    public bool IsCppCalculationSelected =>
        SelectedCalculation?.Name.Equals(DefaultPayrollCalculations.CppName, StringComparison.OrdinalIgnoreCase) == true;

    public bool IsEiCalculationSelected =>
        SelectedCalculation?.Name.Equals(DefaultPayrollCalculations.EiName, StringComparison.OrdinalIgnoreCase) == true;

    public bool IsTaxCalculationSelected =>
        SelectedCalculation?.Name.Equals(DefaultPayrollCalculations.TaxName, StringComparison.OrdinalIgnoreCase) == true;

    public bool ShowBasicGuidedFields =>
        IsCppCalculationSelected || IsEiCalculationSelected;

    public bool ShowHealthPremiumBands => HealthPremiumBands.Count > 0;

    public string NewCalculationName
    {
        get => _newCalculationName;
        set => SetProperty(ref _newCalculationName, value);
    }

    public string NewCalculation
    {
        get => _newCalculation;
        set => SetProperty(ref _newCalculation, value);
    }

    private async Task LoadInitialSettingsAsync()
    {
        await LoadSavedSchedulesAsync();
        await LoadCalculationsAsync();
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

    private async Task LoadCalculationsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Message = "";

            await LoadCalculationsIntoCollectionAsync();
            Message = "Loaded payroll calculations.";
        }
        catch (Exception ex)
        {
            Message = $"Unable to load payroll calculations: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveSelectedCalculationAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (SelectedCalculation is null)
        {
            Message = "Select a calculation to save.";
            return;
        }

        try
        {
            IsBusy = true;
            Message = "";

            ApplyGuidedEditorToSelectedCalculation();

            await _calculationSettingsService.SaveCalculationAsync(new CalculationDto
            {
                Id = SelectedCalculation.Id,
                Name = SelectedCalculation.Name,
                Calculation = SelectedCalculation.Calculation
            });
            await LoadCalculationsIntoCollectionAsync(SelectedCalculation.Name);

            Message = $"Saved {SelectedCalculation?.Name ?? "calculation"}.";
        }
        catch (Exception ex)
        {
            Message = $"Unable to save calculation: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddCalculationAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Message = "";

            await _calculationSettingsService.SaveCalculationAsync(new CalculationDto
            {
                Name = NewCalculationName,
                Calculation = NewCalculation
            });

            var addedName = NewCalculationName.Trim();
            NewCalculationName = "";
            NewCalculation = "";
            await LoadCalculationsIntoCollectionAsync(addedName);

            Message = $"Added {addedName}.";
        }
        catch (Exception ex)
        {
            Message = $"Unable to add calculation: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadCalculationsIntoCollectionAsync(string? selectedName = null)
    {
        var calculations = await _calculationSettingsService.GetCalculationsAsync();

        Calculations.Clear();

        foreach (var calculation in calculations)
        {
            Calculations.Add(new CalculationEditorRow(
                calculation.Id,
                calculation.Name,
                calculation.Calculation));
        }

        SelectedCalculation = string.IsNullOrWhiteSpace(selectedName)
            ? Calculations.FirstOrDefault()
            : Calculations.FirstOrDefault(calculation =>
                calculation.Name.Equals(selectedName, StringComparison.OrdinalIgnoreCase))
                ?? Calculations.FirstOrDefault();
    }

    private void LoadGuidedCalculationEditor()
    {
        ClearGuidedCalculationEditor();
        NotifyGuidedEditorChanged();

        if (SelectedCalculation is null)
        {
            return;
        }

        if (!IsKnownCalculationSelected)
        {
            return;
        }

        try
        {
            _guidedCalculationJson = JsonNode.Parse(SelectedCalculation.Calculation)?.AsObject()
                ?? throw new InvalidOperationException("Calculation is empty.");
            _guidedPeriodJson = GetLatestPeriod(_guidedCalculationJson);

            if (_guidedPeriodJson is null)
            {
                throw new InvalidOperationException("Calculation does not have any effective periods.");
            }

            GuidedEffectiveDate = ReadDate(_guidedPeriodJson, "effectiveFrom") ?? DateTime.Today;

            if (IsCppCalculationSelected)
            {
                LoadCppEditor();
            }
            else if (IsEiCalculationSelected)
            {
                LoadEiEditor();
            }
            else if (IsTaxCalculationSelected)
            {
                LoadTaxEditor();
            }
        }
        catch (Exception ex)
        {
            Message = $"Unable to open the simple editor for {SelectedCalculation.Name}: {ex.Message}";
            ClearGuidedCalculationEditor();
        }
        finally
        {
            NotifyGuidedEditorChanged();
        }
    }

    private void ClearGuidedCalculationEditor()
    {
        _guidedCalculationJson = null;
        _guidedPeriodJson = null;
        GuidedFields.Clear();
        FederalFields.Clear();
        FederalTaxBrackets.Clear();
        ProvinceCodes.Clear();
        ProvinceFields.Clear();
        ProvinceTaxBrackets.Clear();
        HealthPremiumBands.Clear();
        _selectedProvinceCode = "";
        OnPropertyChanged(nameof(SelectedProvinceCode));
    }

    private void LoadCppEditor()
    {
        if (_guidedPeriodJson is null)
        {
            return;
        }

        AddField(GuidedFields, _guidedPeriodJson, "ympe", "CPP maximum pensionable earnings");
        AddField(GuidedFields, _guidedPeriodJson, "yampe", "CPP2 maximum earnings");
        AddField(GuidedFields, _guidedPeriodJson, "basicExemption", "Annual CPP exemption");
        AddField(GuidedFields, _guidedPeriodJson, "totalRate", "CPP employee rate (%)", isPercent: true);
        AddField(GuidedFields, _guidedPeriodJson, "baseRate", "Base CPP rate (%)", isPercent: true);
        AddField(GuidedFields, _guidedPeriodJson, "additionalRate", "Additional CPP rate (%)", isPercent: true);
        AddField(GuidedFields, _guidedPeriodJson, "cpp2Rate", "CPP2 employee rate (%)", isPercent: true);
        AddField(GuidedFields, _guidedPeriodJson, "maximumContribution", "Maximum CPP1 contribution");
        AddField(GuidedFields, _guidedPeriodJson, "maximumBaseContribution", "Maximum base CPP contribution");
        AddField(GuidedFields, _guidedPeriodJson, "maximumCpp2Contribution", "Maximum CPP2 contribution");
    }

    private void LoadEiEditor()
    {
        if (_guidedPeriodJson is null)
        {
            return;
        }

        AddField(GuidedFields, _guidedPeriodJson, "maximumInsurableEarnings", "Maximum EI insurable earnings");
        AddField(GuidedFields, _guidedPeriodJson, "employeeRate", "EI employee rate (%)", isPercent: true);
        AddField(GuidedFields, _guidedPeriodJson, "maximumEmployeePremium", "Maximum employee EI premium");
    }

    private void LoadTaxEditor()
    {
        if (_guidedPeriodJson is null || _guidedCalculationJson is null)
        {
            return;
        }

        AddField(GuidedFields, _guidedCalculationJson, "defaultProvince", "Fallback province if employee province is blank", isText: true);

        var federal = _guidedPeriodJson["federal"]?.AsObject()
            ?? throw new InvalidOperationException("Federal tax settings are missing.");

        AddField(FederalFields, federal, "basicPersonalAmountMaximum", "Federal basic personal amount - maximum");
        AddField(FederalFields, federal, "basicPersonalAmountMinimum", "Federal basic personal amount - minimum");
        AddField(FederalFields, federal, "basicPersonalAmountPhaseoutStart", "Federal phase-out starts at income");
        AddField(FederalFields, federal, "basicPersonalAmountPhaseoutEnd", "Federal phase-out ends at income");
        AddField(FederalFields, federal, "canadaEmploymentAmount", "Canada employment amount");
        LoadBrackets(federal, FederalTaxBrackets);

        var provinces = _guidedPeriodJson["provinces"]?.AsObject()
            ?? throw new InvalidOperationException("Province tax settings are missing.");
        var defaultProvince = ReadString(_guidedCalculationJson, "defaultProvince");

        foreach (var provinceCode in provinces.Select(item => item.Key).OrderBy(code => code))
        {
            ProvinceCodes.Add(provinceCode);
        }

        _selectedProvinceCode = ProvinceCodes.Contains(defaultProvince)
            ? defaultProvince
            : ProvinceCodes.FirstOrDefault() ?? "";
        OnPropertyChanged(nameof(SelectedProvinceCode));
        LoadSelectedProvinceEditor();
    }

    private void LoadSelectedProvinceEditor()
    {
        ProvinceFields.Clear();
        ProvinceTaxBrackets.Clear();
        HealthPremiumBands.Clear();

        if (!IsTaxCalculationSelected || _guidedPeriodJson is null || string.IsNullOrWhiteSpace(SelectedProvinceCode))
        {
            NotifyGuidedEditorChanged();
            return;
        }

        var provinces = _guidedPeriodJson["provinces"]?.AsObject();
        var province = provinces?[SelectedProvinceCode]?.AsObject();

        if (province is null)
        {
            NotifyGuidedEditorChanged();
            return;
        }

        AddField(ProvinceFields, province, "basicPersonalAmount", $"{SelectedProvinceCode} basic personal amount");

        if (SelectedProvinceCode == "ON")
        {
            AddField(ProvinceFields, province, "taxReductionBasicAmount", "Ontario tax reduction amount");
            AddField(ProvinceFields, province, "surtaxFirstThreshold", "Ontario surtax first threshold");
            AddField(ProvinceFields, province, "surtaxFirstRate", "Ontario first surtax rate (%)", isPercent: true);
            AddField(ProvinceFields, province, "surtaxSecondThreshold", "Ontario surtax second threshold");
            AddField(ProvinceFields, province, "surtaxSecondRate", "Ontario second surtax rate (%)", isPercent: true);
            LoadHealthPremiumBands(province);
        }
        else if (SelectedProvinceCode == "BC")
        {
            AddField(ProvinceFields, province, "taxReductionBasicAmount", "BC tax reduction amount");
            AddField(ProvinceFields, province, "taxReductionStart", "BC tax reduction starts at income");
            AddField(ProvinceFields, province, "taxReductionEnd", "BC tax reduction ends at income");
            AddField(ProvinceFields, province, "taxReductionRate", "BC tax reduction rate (%)", isPercent: true);
        }

        LoadBrackets(province, ProvinceTaxBrackets);
        NotifyGuidedEditorChanged();
    }

    private void ApplyGuidedEditorToSelectedCalculation()
    {
        if (SelectedCalculation is null || !IsKnownCalculationSelected || _guidedCalculationJson is null || _guidedPeriodJson is null)
        {
            return;
        }

        _guidedPeriodJson["effectiveFrom"] = GuidedEffectiveDate.ToString("yyyy-MM-dd");

        foreach (var field in GuidedFields.Concat(FederalFields).Concat(ProvinceFields))
        {
            field.ApplyToJson();
        }

        foreach (var bracket in FederalTaxBrackets.Concat(ProvinceTaxBrackets))
        {
            bracket.ApplyToJson();
        }

        foreach (var band in HealthPremiumBands)
        {
            band.ApplyToJson();
        }

        SelectedCalculation.Calculation = _guidedCalculationJson.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static void AddField(
        ObservableCollection<CalculationSimpleField> fields,
        JsonObject owner,
        string key,
        string label,
        bool isPercent = false,
        bool isText = false)
    {
        fields.Add(new CalculationSimpleField(owner, key, label, isPercent, isText));
    }

    private static void LoadBrackets(
        JsonObject owner,
        ObservableCollection<TaxBracketEditorRow> target)
    {
        target.Clear();

        if (owner["brackets"] is not JsonArray brackets)
        {
            return;
        }

        foreach (var bracket in brackets.OfType<JsonObject>())
        {
            target.Add(new TaxBracketEditorRow(bracket));
        }
    }

    private void LoadHealthPremiumBands(JsonObject province)
    {
        if (province["healthPremium"] is not JsonArray bands)
        {
            return;
        }

        foreach (var band in bands.OfType<JsonObject>())
        {
            HealthPremiumBands.Add(new HealthPremiumEditorRow(band));
        }
    }

    private static JsonObject? GetLatestPeriod(JsonObject calculation)
    {
        return calculation["periods"]?
            .AsArray()
            .OfType<JsonObject>()
            .OrderByDescending(period => ReadDate(period, "effectiveFrom") ?? DateTime.MinValue)
            .FirstOrDefault();
    }

    private void NotifyGuidedEditorChanged()
    {
        OnPropertyChanged(nameof(IsKnownCalculationSelected));
        OnPropertyChanged(nameof(IsCppCalculationSelected));
        OnPropertyChanged(nameof(IsEiCalculationSelected));
        OnPropertyChanged(nameof(IsTaxCalculationSelected));
        OnPropertyChanged(nameof(ShowBasicGuidedFields));
        OnPropertyChanged(nameof(ShowHealthPremiumBands));
    }

    private static string ReadString(JsonObject owner, string key) =>
        owner[key]?.GetValue<string>() ?? "";

    private static DateTime? ReadDate(JsonObject owner, string key)
    {
        var value = ReadString(owner, key);
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.Date
            : null;
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

public sealed class CalculationEditorRow : INotifyPropertyChanged
{
    private string _name;
    private string _calculation;

    public CalculationEditorRow(int id, string name, string calculation)
    {
        Id = id;
        _name = name;
        _calculation = calculation;
    }

    public int Id { get; }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string DisplayName => Name switch
    {
        DefaultPayrollCalculations.CppName => "CPP",
        DefaultPayrollCalculations.EiName => "EI",
        DefaultPayrollCalculations.TaxName => "Income Tax",
        _ => Name
    };

    public string Calculation
    {
        get => _calculation;
        set => SetProperty(ref _calculation, value);
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
}

public sealed class CalculationSimpleField : INotifyPropertyChanged
{
    private readonly JsonObject _owner;
    private readonly string _key;
    private readonly bool _isPercent;
    private readonly bool _isText;
    private string _value;

    public CalculationSimpleField(
        JsonObject owner,
        string key,
        string label,
        bool isPercent,
        bool isText)
    {
        _owner = owner;
        _key = key;
        _isPercent = isPercent;
        _isText = isText;
        Label = label;
        _value = FormatValue(owner[key], isPercent, isText);
    }

    public string Label { get; }

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public void ApplyToJson()
    {
        if (_isText)
        {
            _owner[_key] = Value.Trim().ToUpperInvariant();
            return;
        }

        var amount = ParseDecimal(Value, Label);
        _owner[_key] = _isPercent ? amount / 100m : amount;
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

    internal static string FormatValue(JsonNode? node, bool isPercent = false, bool isText = false)
    {
        if (node is null)
        {
            return "";
        }

        if (isText)
        {
            return node.GetValue<string>();
        }

        var value = node.GetValue<decimal>();
        var displayValue = isPercent ? value * 100m : value;

        return displayValue.ToString("0.####", CultureInfo.InvariantCulture);
    }

    internal static decimal ParseDecimal(string value, string label)
    {
        if (decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out var currentCultureValue))
        {
            return currentCultureValue;
        }

        if (decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var invariantCultureValue))
        {
            return invariantCultureValue;
        }

        throw new InvalidOperationException($"{label} must be a number.");
    }
}

public sealed class TaxBracketEditorRow : INotifyPropertyChanged
{
    private readonly JsonObject _owner;
    private string _threshold;
    private string _rate;
    private string _constant;

    public TaxBracketEditorRow(JsonObject owner)
    {
        _owner = owner;
        _threshold = CalculationSimpleField.FormatValue(owner["threshold"]);
        _rate = CalculationSimpleField.FormatValue(owner["rate"], isPercent: true);
        _constant = CalculationSimpleField.FormatValue(owner["constant"]);
    }

    public string Threshold
    {
        get => _threshold;
        set => SetProperty(ref _threshold, value);
    }

    public string Rate
    {
        get => _rate;
        set => SetProperty(ref _rate, value);
    }

    public string Constant
    {
        get => _constant;
        set => SetProperty(ref _constant, value);
    }

    public void ApplyToJson()
    {
        _owner["threshold"] = CalculationSimpleField.ParseDecimal(Threshold, "Bracket starts at");
        _owner["rate"] = CalculationSimpleField.ParseDecimal(Rate, "Tax rate") / 100m;
        _owner["constant"] = CalculationSimpleField.ParseDecimal(Constant, "Subtract amount");
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
}

public sealed class HealthPremiumEditorRow : INotifyPropertyChanged
{
    private readonly JsonObject _owner;
    private string _start;
    private string _end;
    private string _base;
    private string _rate;
    private string _maximum;

    public HealthPremiumEditorRow(JsonObject owner)
    {
        _owner = owner;
        _start = CalculationSimpleField.FormatValue(owner["start"]);
        _end = owner["end"] is null ? "" : CalculationSimpleField.FormatValue(owner["end"]);
        _base = CalculationSimpleField.FormatValue(owner["base"]);
        _rate = CalculationSimpleField.FormatValue(owner["rate"], isPercent: true);
        _maximum = CalculationSimpleField.FormatValue(owner["maximum"]);
    }

    public string Start
    {
        get => _start;
        set => SetProperty(ref _start, value);
    }

    public string End
    {
        get => _end;
        set => SetProperty(ref _end, value);
    }

    public string Base
    {
        get => _base;
        set => SetProperty(ref _base, value);
    }

    public string Rate
    {
        get => _rate;
        set => SetProperty(ref _rate, value);
    }

    public string Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }

    public void ApplyToJson()
    {
        _owner["start"] = CalculationSimpleField.ParseDecimal(Start, "Health premium starts at");
        _owner["end"] = string.IsNullOrWhiteSpace(End)
            ? null
            : CalculationSimpleField.ParseDecimal(End, "Health premium ends at");
        _owner["base"] = CalculationSimpleField.ParseDecimal(Base, "Health premium base");
        _owner["rate"] = CalculationSimpleField.ParseDecimal(Rate, "Health premium rate") / 100m;
        _owner["maximum"] = CalculationSimpleField.ParseDecimal(Maximum, "Health premium maximum");
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
}
