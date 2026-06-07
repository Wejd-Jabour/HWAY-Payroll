namespace HWAY.Payroll.Services.T4;

public sealed record T4TaxYearRules(
    int Year,
    decimal CppMaximumPensionableEarnings,
    decimal CppAdditionalMaximumPensionableEarnings,
    decimal Cpp1MaximumContribution,
    decimal Cpp2MaximumContribution,
    decimal EiMaximumInsurableEarnings,
    decimal EiMaximumPremium)
{
    public decimal Box26PensionableEarningsCap =>
        Year >= 2024
            ? CppAdditionalMaximumPensionableEarnings
            : CppMaximumPensionableEarnings;
}

public static class T4TaxYearRuleProvider
{
    private static readonly IReadOnlyDictionary<int, T4TaxYearRules> Rules =
        new Dictionary<int, T4TaxYearRules>
        {
            [2024] = new(
                2024,
                CppMaximumPensionableEarnings: 68500m,
                CppAdditionalMaximumPensionableEarnings: 73200m,
                Cpp1MaximumContribution: 3867.50m,
                Cpp2MaximumContribution: 188.00m,
                EiMaximumInsurableEarnings: 63200m,
                EiMaximumPremium: 1049.12m),
            [2025] = new(
                2025,
                CppMaximumPensionableEarnings: 71300m,
                CppAdditionalMaximumPensionableEarnings: 81200m,
                Cpp1MaximumContribution: 4034.10m,
                Cpp2MaximumContribution: 396.00m,
                EiMaximumInsurableEarnings: 65700m,
                EiMaximumPremium: 1077.48m),
            [2026] = new(
                2026,
                CppMaximumPensionableEarnings: 74600m,
                CppAdditionalMaximumPensionableEarnings: 85000m,
                Cpp1MaximumContribution: 4230.45m,
                Cpp2MaximumContribution: 416.00m,
                EiMaximumInsurableEarnings: 68900m,
                EiMaximumPremium: 1123.07m)
        };

    public static IReadOnlyList<int> SupportedYears { get; } = Rules.Keys
        .OrderByDescending(year => year)
        .ToList();

    public static T4TaxYearRules? GetRules(int year) =>
        Rules.TryGetValue(year, out var rules)
            ? rules
            : null;
}
