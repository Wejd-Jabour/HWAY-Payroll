using System.Text.Json;

namespace HWAY.Payroll.Services.Calculations;

public sealed class PayrollDeductionCalculationRequest
{
    public DateTime PayDate { get; set; } = DateTime.Today;

    public int PayPeriodsPerYear { get; set; }

    public int CurrentPeriod { get; set; }

    public string Province { get; set; } = string.Empty;

    public decimal GrossIncome { get; set; }

    public decimal PaidVacation { get; set; }

    public decimal PensionableIncomeYtd { get; set; }

    public decimal Cpp1Ytd { get; set; }

    public decimal Cpp2Ytd { get; set; }

    public decimal EiYtd { get; set; }

    public bool CanContributeCpp { get; set; } = true;

    public bool CanContributeEi { get; set; } = true;
}

public sealed record PayrollDeductionCalculationResult(
    decimal Tax,
    decimal Cpp1,
    decimal Cpp2,
    decimal Ei);

public interface IPayrollDeductionCalculator
{
    Task<PayrollDeductionCalculationResult> CalculateAsync(
        PayrollDeductionCalculationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class PayrollDeductionCalculator : IPayrollDeductionCalculator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static readonly IReadOnlyDictionary<string, string> ProvinceAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ALBERTA"] = "AB",
            ["BRITISH COLUMBIA"] = "BC",
            ["MANITOBA"] = "MB",
            ["NEW BRUNSWICK"] = "NB",
            ["NEWFOUNDLAND"] = "NL",
            ["NEWFOUNDLAND AND LABRADOR"] = "NL",
            ["NOVA SCOTIA"] = "NS",
            ["NORTHWEST TERRITORIES"] = "NT",
            ["NUNAVUT"] = "NU",
            ["ONTARIO"] = "ON",
            ["PRINCE EDWARD ISLAND"] = "PE",
            ["SASKATCHEWAN"] = "SK",
            ["YUKON"] = "YT",
            ["YUKON TERRITORY"] = "YT"
        };

    private readonly ICalculationSettingsService _calculationSettingsService;

    public PayrollDeductionCalculator(ICalculationSettingsService calculationSettingsService)
    {
        _calculationSettingsService = calculationSettingsService;
    }

    public async Task<PayrollDeductionCalculationResult> CalculateAsync(
        PayrollDeductionCalculationRequest request,
        CancellationToken cancellationToken = default)
    {
        var definitions = await LoadDefinitionsAsync(cancellationToken);
        var cppPeriod = SelectPeriod(definitions.Cpp.Periods, request.PayDate);
        var eiPeriod = SelectPeriod(definitions.Ei.Periods, request.PayDate);
        var taxPeriod = SelectPeriod(definitions.Tax.Periods, request.PayDate);
        var payPeriods = request.PayPeriodsPerYear > 0 ? request.PayPeriodsPerYear : 26;
        var totalPay = Math.Max(0m, request.GrossIncome + request.PaidVacation);

        var cpp1 = request.CanContributeCpp
            ? CalculateCpp1(totalPay, request.Cpp1Ytd, payPeriods, cppPeriod)
            : 0m;
        var cpp2 = request.CanContributeCpp
            ? CalculateCpp2(totalPay, request.PensionableIncomeYtd, request.Cpp2Ytd, cppPeriod)
            : 0m;
        var ei = request.CanContributeEi
            ? CalculateEi(totalPay, request.EiYtd, eiPeriod)
            : 0m;
        var tax = CalculateTax(
            request,
            totalPay,
            cpp1,
            cpp2,
            ei,
            payPeriods,
            cppPeriod,
            eiPeriod,
            taxPeriod,
            definitions.Tax.DefaultProvince);

        return new PayrollDeductionCalculationResult(tax, cpp1, cpp2, ei);
    }

    private async Task<PayrollFormulaDefinitions> LoadDefinitionsAsync(CancellationToken cancellationToken)
    {
        var calculations = await _calculationSettingsService.GetCalculationsAsync(cancellationToken);

        var cpp = Deserialize<CppCalculationDefinition>(
            calculations,
            DefaultPayrollCalculations.CppName,
            DefaultPayrollCalculations.CppCalculation);
        var ei = Deserialize<EiCalculationDefinition>(
            calculations,
            DefaultPayrollCalculations.EiName,
            DefaultPayrollCalculations.EiCalculation);
        var tax = Deserialize<TaxCalculationDefinition>(
            calculations,
            DefaultPayrollCalculations.TaxName,
            DefaultPayrollCalculations.TaxCalculation);

        if (cpp.Periods.Count == 0)
        {
            throw new InvalidOperationException("CPP calculation must contain at least one period.");
        }

        if (ei.Periods.Count == 0)
        {
            throw new InvalidOperationException("EI calculation must contain at least one period.");
        }

        if (tax.Periods.Count == 0)
        {
            throw new InvalidOperationException("Income tax calculation must contain at least one period.");
        }

        return new PayrollFormulaDefinitions(cpp, ei, tax);
    }

    private static T Deserialize<T>(
        IReadOnlyList<CalculationDto> calculations,
        string name,
        string fallback)
    {
        var json = calculations
            .FirstOrDefault(calculation => calculation.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?.Calculation;

        try
        {
            return JsonSerializer.Deserialize<T>(
                    string.IsNullOrWhiteSpace(json) ? fallback : json,
                    JsonOptions)
                ?? throw new InvalidOperationException($"{name} calculation is empty.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"{name} calculation JSON is invalid: {ex.Message}", ex);
        }
    }

    private static T SelectPeriod<T>(IReadOnlyList<T> periods, DateTime payDate)
        where T : EffectivePeriod
    {
        var selected = periods
            .Where(period => period.EffectiveFrom.Date <= payDate.Date)
            .OrderByDescending(period => period.EffectiveFrom)
            .FirstOrDefault();

        return selected ?? periods.OrderBy(period => period.EffectiveFrom).First();
    }

    private static decimal CalculateCpp1(
        decimal pensionableIncome,
        decimal cpp1Ytd,
        int payPeriods,
        CppFormulaPeriod period)
    {
        var basicExemption = FloorCents(period.BasicExemption / payPeriods);
        var contribution = RoundCents(period.TotalRate * Math.Max(0m, pensionableIncome - basicExemption));
        var remaining = Math.Max(0m, period.MaximumContribution * period.MonthsProrated / 12m - cpp1Ytd);

        return Math.Min(contribution, remaining);
    }

    private static decimal CalculateCpp2(
        decimal pensionableIncome,
        decimal pensionableIncomeYtd,
        decimal cpp2Ytd,
        CppFormulaPeriod period)
    {
        var threshold = period.Ympe * period.MonthsProrated / 12m;
        var w = Math.Max(pensionableIncomeYtd, threshold);
        var contribution = RoundCents(Math.Max(0m, pensionableIncomeYtd + pensionableIncome - w) * period.Cpp2Rate);
        var remaining = Math.Max(0m, period.MaximumCpp2Contribution * period.MonthsProrated / 12m - cpp2Ytd);

        return Math.Min(contribution, remaining);
    }

    private static decimal CalculateEi(
        decimal insurableIncome,
        decimal eiYtd,
        EiFormulaPeriod period)
    {
        var premium = RoundCents(Math.Max(0m, insurableIncome) * period.EmployeeRate);
        var remaining = Math.Max(0m, period.MaximumEmployeePremium - eiYtd);

        return Math.Min(premium, remaining);
    }

    private static decimal CalculateTax(
        PayrollDeductionCalculationRequest request,
        decimal totalPay,
        decimal cpp1,
        decimal cpp2,
        decimal ei,
        int payPeriods,
        CppFormulaPeriod cppPeriod,
        EiFormulaPeriod eiPeriod,
        TaxFormulaPeriod taxPeriod,
        string defaultProvince)
    {
        var provinceCode = NormalizeProvince(request.Province, defaultProvince);

        if (!taxPeriod.Provinces.TryGetValue(provinceCode, out var province))
        {
            provinceCode = NormalizeProvince(defaultProvince, "ON");
            taxPeriod.Provinces.TryGetValue(provinceCode, out province);
        }

        if (province is null)
        {
            throw new InvalidOperationException(
                $"Income tax calculation does not contain province '{provinceCode}'.");
        }

        var enhancedCppDeduction = cppPeriod.TotalRate == 0
            ? 0m
            : cpp1 * (cppPeriod.AdditionalRate / cppPeriod.TotalRate) + cpp2;
        var annualTaxableIncome = Math.Max(0m, payPeriods * Math.Max(0m, totalPay - enhancedCppDeduction));
        var annualEmploymentIncome = Math.Max(0m, payPeriods * totalPay);
        var annualBaseCppCredit = CalculateAnnualBaseCppForTaxCredit(
            cpp1,
            request.Cpp1Ytd,
            payPeriods,
            cppPeriod);
        var annualEiCredit = CalculateAnnualEiForTaxCredit(ei, request.EiYtd, payPeriods, eiPeriod);
        var federalTax = CalculateFederalTax(
            annualTaxableIncome,
            annualEmploymentIncome,
            annualBaseCppCredit,
            annualEiCredit,
            cppPeriod,
            taxPeriod.Federal);
        var provincialTax = CalculateProvincialTax(
            provinceCode,
            province,
            annualTaxableIncome,
            annualEmploymentIncome,
            annualBaseCppCredit,
            annualEiCredit,
            taxPeriod.Federal.CanadaEmploymentAmount);

        return RoundCents(Math.Max(0m, federalTax + provincialTax) / payPeriods);
    }

    private static decimal CalculateFederalTax(
        decimal annualTaxableIncome,
        decimal annualEmploymentIncome,
        decimal annualBaseCppCredit,
        decimal annualEiCredit,
        CppFormulaPeriod cppPeriod,
        FederalTaxFormula federal)
    {
        var bracket = FindBracket(federal.Brackets, annualTaxableIncome);
        var lowestRate = federal.Brackets.OrderBy(item => item.Threshold).First().Rate;
        var basicPersonalAmount = CalculateFederalBasicPersonalAmount(annualTaxableIncome, federal);
        var k1 = lowestRate * basicPersonalAmount;
        var k2 = lowestRate * (annualBaseCppCredit + annualEiCredit);
        var k4 = lowestRate * Math.Min(annualEmploymentIncome, federal.CanadaEmploymentAmount);

        return Math.Max(0m, bracket.Rate * annualTaxableIncome - bracket.Constant - k1 - k2 - k4);
    }

    private static decimal CalculateProvincialTax(
        string provinceCode,
        ProvinceTaxFormula province,
        decimal annualTaxableIncome,
        decimal annualEmploymentIncome,
        decimal annualBaseCppCredit,
        decimal annualEiCredit,
        decimal federalCanadaEmploymentAmount)
    {
        var bracket = FindBracket(province.Brackets, annualTaxableIncome);
        var lowestRate = province.Brackets.OrderBy(item => item.Threshold).First().Rate;
        var k1p = lowestRate * province.BasicPersonalAmount;
        var k2p = lowestRate * (annualBaseCppCredit + annualEiCredit);
        var k4p = province.TerritorialCanadaEmploymentAmount
            ? lowestRate * Math.Min(annualEmploymentIncome, federalCanadaEmploymentAmount)
            : 0m;
        var t4 = Math.Max(
            0m,
            bracket.Rate * annualTaxableIncome - bracket.Constant - k1p - k2p - k4p);

        var surtax = provinceCode == "ON" ? CalculateOntarioSurtax(t4, province) : 0m;
        var healthPremium = provinceCode == "ON"
            ? CalculateOntarioHealthPremium(annualTaxableIncome, province)
            : 0m;
        var reduction = CalculateProvincialReduction(provinceCode, t4, surtax, annualTaxableIncome, province);

        return Math.Max(0m, t4 + surtax + healthPremium - reduction);
    }

    private static decimal CalculateAnnualBaseCppForTaxCredit(
        decimal cpp1,
        decimal cpp1Ytd,
        int payPeriods,
        CppFormulaPeriod period)
    {
        if (period.TotalRate == 0)
        {
            return 0m;
        }

        var baseRatio = period.BaseRate / period.TotalRate;
        var currentBase = cpp1 * baseRatio;

        if (cpp1Ytd + cpp1 >= period.MaximumContribution)
        {
            return period.MaximumBaseContribution * period.MonthsProrated / 12m;
        }

        return Math.Min(
            period.MaximumBaseContribution * period.MonthsProrated / 12m,
            Math.Max(payPeriods * currentBase, cpp1Ytd * baseRatio + currentBase));
    }

    private static decimal CalculateAnnualEiForTaxCredit(
        decimal ei,
        decimal eiYtd,
        int payPeriods,
        EiFormulaPeriod period)
    {
        if (eiYtd + ei >= period.MaximumEmployeePremium)
        {
            return period.MaximumEmployeePremium;
        }

        return Math.Min(period.MaximumEmployeePremium, Math.Max(payPeriods * ei, eiYtd + ei));
    }

    private static decimal CalculateFederalBasicPersonalAmount(
        decimal annualTaxableIncome,
        FederalTaxFormula federal)
    {
        if (annualTaxableIncome <= federal.BasicPersonalAmountPhaseoutStart)
        {
            return federal.BasicPersonalAmountMaximum;
        }

        if (annualTaxableIncome >= federal.BasicPersonalAmountPhaseoutEnd)
        {
            return federal.BasicPersonalAmountMinimum;
        }

        var range = federal.BasicPersonalAmountPhaseoutEnd - federal.BasicPersonalAmountPhaseoutStart;
        var reduction = (annualTaxableIncome - federal.BasicPersonalAmountPhaseoutStart)
            * ((federal.BasicPersonalAmountMaximum - federal.BasicPersonalAmountMinimum) / range);

        return RoundCents(federal.BasicPersonalAmountMaximum - reduction);
    }

    private static decimal CalculateOntarioSurtax(decimal t4, ProvinceTaxFormula province)
    {
        var first = province.SurtaxFirstThreshold.HasValue
            ? Math.Max(0m, t4 - province.SurtaxFirstThreshold.Value) * province.SurtaxFirstRate
            : 0m;
        var second = province.SurtaxSecondThreshold.HasValue
            ? Math.Max(0m, t4 - province.SurtaxSecondThreshold.Value) * province.SurtaxSecondRate
            : 0m;

        return first + second;
    }

    private static decimal CalculateOntarioHealthPremium(
        decimal annualTaxableIncome,
        ProvinceTaxFormula province)
    {
        foreach (var band in province.HealthPremium.OrderBy(item => item.Start))
        {
            if (annualTaxableIncome <= band.Start)
            {
                continue;
            }

            if (band.End.HasValue && annualTaxableIncome > band.End.Value)
            {
                continue;
            }

            return Math.Min(
                band.Maximum,
                band.Base + (annualTaxableIncome - band.Start) * band.Rate);
        }

        return 0m;
    }

    private static decimal CalculateProvincialReduction(
        string provinceCode,
        decimal t4,
        decimal surtax,
        decimal annualTaxableIncome,
        ProvinceTaxFormula province)
    {
        if (provinceCode == "ON" && province.TaxReductionBasicAmount.HasValue)
        {
            var taxBeforeReduction = t4 + surtax;
            var reduction = 2m * province.TaxReductionBasicAmount.Value - taxBeforeReduction;
            return Math.Max(0m, Math.Min(taxBeforeReduction, reduction));
        }

        if (provinceCode == "BC" &&
            province.TaxReductionBasicAmount.HasValue &&
            province.TaxReductionStart.HasValue &&
            province.TaxReductionEnd.HasValue &&
            province.TaxReductionRate.HasValue)
        {
            if (annualTaxableIncome <= province.TaxReductionStart.Value)
            {
                return Math.Min(t4, province.TaxReductionBasicAmount.Value);
            }

            if (annualTaxableIncome <= province.TaxReductionEnd.Value)
            {
                var reduction = province.TaxReductionBasicAmount.Value
                    - (annualTaxableIncome - province.TaxReductionStart.Value)
                    * province.TaxReductionRate.Value;

                return Math.Max(0m, Math.Min(t4, reduction));
            }
        }

        return 0m;
    }

    private static TaxBracket FindBracket(IReadOnlyList<TaxBracket> brackets, decimal annualTaxableIncome)
    {
        if (brackets.Count == 0)
        {
            throw new InvalidOperationException("Tax bracket list cannot be empty.");
        }

        return brackets
            .OrderByDescending(bracket => bracket.Threshold)
            .First(bracket => annualTaxableIncome >= bracket.Threshold);
    }

    private static string NormalizeProvince(string? province, string defaultProvince)
    {
        var normalizedDefault = string.IsNullOrWhiteSpace(defaultProvince)
            ? "ON"
            : defaultProvince.Trim().ToUpperInvariant();
        var value = province?.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            return normalizedDefault;
        }

        var compact = value.Replace(".", "", StringComparison.Ordinal).ToUpperInvariant();

        if (compact.Length == 2)
        {
            return compact;
        }

        return ProvinceAliases.TryGetValue(compact, out var provinceCode)
            ? provinceCode
            : normalizedDefault;
    }

    private static decimal FloorCents(decimal value) => Math.Floor(value * 100m) / 100m;

    private static decimal RoundCents(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private sealed record PayrollFormulaDefinitions(
        CppCalculationDefinition Cpp,
        EiCalculationDefinition Ei,
        TaxCalculationDefinition Tax);

    private abstract class EffectivePeriod
    {
        public DateTime EffectiveFrom { get; set; }
    }

    private sealed class CppCalculationDefinition
    {
        public List<CppFormulaPeriod> Periods { get; set; } = new();
    }

    private sealed class CppFormulaPeriod : EffectivePeriod
    {
        public decimal Ympe { get; set; }
        public decimal Yampe { get; set; }
        public decimal BasicExemption { get; set; }
        public decimal TotalRate { get; set; }
        public decimal BaseRate { get; set; }
        public decimal AdditionalRate { get; set; }
        public decimal Cpp2Rate { get; set; }
        public decimal MaximumContribution { get; set; }
        public decimal MaximumBaseContribution { get; set; }
        public decimal MaximumCpp2Contribution { get; set; }
        public decimal MonthsProrated { get; set; } = 12m;
    }

    private sealed class EiCalculationDefinition
    {
        public List<EiFormulaPeriod> Periods { get; set; } = new();
    }

    private sealed class EiFormulaPeriod : EffectivePeriod
    {
        public decimal MaximumInsurableEarnings { get; set; }
        public decimal EmployeeRate { get; set; }
        public decimal MaximumEmployeePremium { get; set; }
    }

    private sealed class TaxCalculationDefinition
    {
        public string DefaultProvince { get; set; } = "ON";
        public List<TaxFormulaPeriod> Periods { get; set; } = new();
    }

    private sealed class TaxFormulaPeriod : EffectivePeriod
    {
        public FederalTaxFormula Federal { get; set; } = new();
        public Dictionary<string, ProvinceTaxFormula> Provinces { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class FederalTaxFormula
    {
        public List<TaxBracket> Brackets { get; set; } = new();
        public decimal BasicPersonalAmountMaximum { get; set; }
        public decimal BasicPersonalAmountMinimum { get; set; }
        public decimal BasicPersonalAmountPhaseoutStart { get; set; }
        public decimal BasicPersonalAmountPhaseoutEnd { get; set; }
        public decimal CanadaEmploymentAmount { get; set; }
    }

    private sealed class ProvinceTaxFormula
    {
        public decimal BasicPersonalAmount { get; set; }
        public bool TerritorialCanadaEmploymentAmount { get; set; }
        public decimal? TaxReductionBasicAmount { get; set; }
        public decimal? TaxReductionStart { get; set; }
        public decimal? TaxReductionEnd { get; set; }
        public decimal? TaxReductionRate { get; set; }
        public decimal? SurtaxFirstThreshold { get; set; }
        public decimal SurtaxFirstRate { get; set; }
        public decimal? SurtaxSecondThreshold { get; set; }
        public decimal SurtaxSecondRate { get; set; }
        public List<TaxBracket> Brackets { get; set; } = new();
        public List<OntarioHealthPremiumBand> HealthPremium { get; set; } = new();
    }

    private sealed class TaxBracket
    {
        public decimal Threshold { get; set; }
        public decimal Rate { get; set; }
        public decimal Constant { get; set; }
    }

    private sealed class OntarioHealthPremiumBand
    {
        public decimal Start { get; set; }
        public decimal? End { get; set; }
        public decimal Base { get; set; }
        public decimal Rate { get; set; }
        public decimal Maximum { get; set; }
    }
}
